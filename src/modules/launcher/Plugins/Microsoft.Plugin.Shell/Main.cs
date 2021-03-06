// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Microsoft.PowerToys.Settings.UI.Lib;
using Wox.Infrastructure.Logger;
using Wox.Infrastructure.Storage;
using Wox.Plugin;
using Wox.Plugin.SharedCommands;
using Control = System.Windows.Controls.Control;

namespace Microsoft.Plugin.Shell
{
    public class Main : IPlugin, ISettingProvider, IPluginI18n, IContextMenu, ISavable
    {
        private readonly Settings _settings;
        private readonly PluginJsonStorage<Settings> _storage;

        private string IconPath { get; set; }

        private PluginInitContext _context;

        public Main()
        {
            _storage = new PluginJsonStorage<Settings>();
            _settings = _storage.Load();
        }

        public void Save()
        {
            _storage.Save();
        }

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            string cmd = query.Search;
            if (string.IsNullOrEmpty(cmd))
            {
                return ResultsFromlHistory();
            }
            else
            {
                var queryCmd = GetCurrentCmd(cmd);
                results.Add(queryCmd);
                var history = GetHistoryCmds(cmd, queryCmd);
                results.AddRange(history);

                try
                {
                    List<Result> folderPluginResults = Folder.Main.GetFolderPluginResults(query);
                    results.AddRange(folderPluginResults);
                }
                catch (Exception e)
                {
                    Log.Exception($"|Microsoft.Plugin.Shell.Main.Query|Exception when query for <{query}>", e);
                }

                return results;
            }
        }

        private List<Result> GetHistoryCmds(string cmd, Result result)
        {
            IEnumerable<Result> history = _settings.Count.Where(o => o.Key.Contains(cmd))
                .OrderByDescending(o => o.Value)
                .Select(m =>
                {
                    if (m.Key == cmd)
                    {
                        result.SubTitle = "Shell: " + string.Format(_context.API.GetTranslation("wox_plugin_cmd_cmd_has_been_executed_times"), m.Value);
                        return null;
                    }

                    var ret = new Result
                    {
                        Title = m.Key,
                        SubTitle = "Shell: " + string.Format(_context.API.GetTranslation("wox_plugin_cmd_cmd_has_been_executed_times"), m.Value),
                        IcoPath = IconPath,
                        Action = c =>
                        {
                            Execute(Process.Start, PrepareProcessStartInfo(m.Key));
                            return true;
                        },
                    };
                    return ret;
                }).Where(o => o != null).Take(4);
            return history.ToList();
        }

        private Result GetCurrentCmd(string cmd)
        {
            Result result = new Result
            {
                Title = cmd,
                Score = 5000,
                SubTitle = "Shell: " + _context.API.GetTranslation("wox_plugin_cmd_execute_through_shell"),
                IcoPath = IconPath,
                Action = c =>
                {
                    Execute(Process.Start, PrepareProcessStartInfo(cmd));
                    return true;
                },
            };

            return result;
        }

        private List<Result> ResultsFromlHistory()
        {
            IEnumerable<Result> history = _settings.Count.OrderByDescending(o => o.Value)
                .Select(m => new Result
                {
                    Title = m.Key,
                    SubTitle = "Shell: " + string.Format(_context.API.GetTranslation("wox_plugin_cmd_cmd_has_been_executed_times"), m.Value),
                    IcoPath = IconPath,
                    Action = c =>
                    {
                        Execute(Process.Start, PrepareProcessStartInfo(m.Key));
                        return true;
                    },
                }).Take(5);
            return history.ToList();
        }

        private ProcessStartInfo PrepareProcessStartInfo(string command, bool runAsAdministrator = false)
        {
            command = command.Trim();
            command = Environment.ExpandEnvironmentVariables(command);
            var workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var runAsAdministratorArg = !runAsAdministrator && !_settings.RunAsAdministrator ? string.Empty : "runas";

            ProcessStartInfo info;
            if (_settings.Shell == Shell.Cmd)
            {
                var arguments = _settings.LeaveShellOpen ? $"/k \"{command}\"" : $"/c \"{command}\" & pause";

                info = ShellCommand.SetProcessStartInfo("cmd.exe", workingDirectory, arguments, runAsAdministratorArg);
            }
            else if (_settings.Shell == Shell.Powershell)
            {
                string arguments;
                if (_settings.LeaveShellOpen)
                {
                    arguments = $"-NoExit \"{command}\"";
                }
                else
                {
                    arguments = $"\"{command} ; Read-Host -Prompt \\\"Press Enter to continue\\\"\"";
                }

                info = ShellCommand.SetProcessStartInfo("powershell.exe", workingDirectory, arguments, runAsAdministratorArg);
            }
            else if (_settings.Shell == Shell.RunCommand)
            {
                // Open explorer if the path is a file or directory
                if (Directory.Exists(command) || File.Exists(command))
                {
                    info = ShellCommand.SetProcessStartInfo("explorer.exe", arguments: command, verb: runAsAdministratorArg);
                }
                else
                {
                    var parts = command.Split(new[] { ' ' }, 2);
                    if (parts.Length == 2)
                    {
                        var filename = parts[0];
                        if (ExistInPath(filename))
                        {
                            var arguments = parts[1];
                            info = ShellCommand.SetProcessStartInfo(filename, workingDirectory, arguments, runAsAdministratorArg);
                        }
                        else
                        {
                            info = ShellCommand.SetProcessStartInfo(command, verb: runAsAdministratorArg);
                        }
                    }
                    else
                    {
                        info = ShellCommand.SetProcessStartInfo(command, verb: runAsAdministratorArg);
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            info.UseShellExecute = true;

            _settings.AddCmdHistory(command);

            return info;
        }

        private void Execute(Func<ProcessStartInfo, Process> startProcess, ProcessStartInfo info)
        {
            try
            {
                startProcess(info);
            }
            catch (FileNotFoundException e)
            {
                var name = "Plugin: Shell";
                var message = $"Command not found: {e.Message}";
                _context.API.ShowMsg(name, message);
            }
            catch (Win32Exception e)
            {
                var name = "Plugin: Shell";
                var message = $"Error running the command: {e.Message}";
                _context.API.ShowMsg(name, message);
            }
        }

        private bool ExistInPath(string filename)
        {
            if (File.Exists(filename))
            {
                return true;
            }
            else
            {
                var values = Environment.GetEnvironmentVariable("PATH");
                if (values != null)
                {
                    foreach (var path in values.Split(';'))
                    {
                        var path1 = Path.Combine(path, filename);
                        var path2 = Path.Combine(path, filename + ".exe");
                        if (File.Exists(path1) || File.Exists(path2))
                        {
                            return true;
                        }
                    }

                    return false;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Init(PluginInitContext context)
        {
            this._context = context;
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(_context.API.GetCurrentTheme());
        }

        // Todo : Update with theme based IconPath
        private void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                IconPath = "Images/shell.light.png";
            }
            else
            {
                IconPath = "Images/shell.dark.png";
            }
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        public Control CreateSettingPanel()
        {
            return new CMDSetting(_settings);
        }

        public string GetTranslatedPluginTitle()
        {
            return _context.API.GetTranslation("wox_plugin_cmd_plugin_name");
        }

        public string GetTranslatedPluginDescription()
        {
            return _context.API.GetTranslation("wox_plugin_cmd_plugin_description");
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            var resultlist = new List<ContextMenuResult>
            {
                new ContextMenuResult
                {
                    PluginName = Assembly.GetExecutingAssembly().GetName().Name,
                    Title = _context.API.GetTranslation("wox_plugin_cmd_run_as_administrator"),
                    Glyph = "\xE7EF",
                    FontFamily = "Segoe MDL2 Assets",
                    AcceleratorKey = Key.Enter,
                    AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                    Action = c =>
                    {
                        Execute(Process.Start, PrepareProcessStartInfo(selectedResult.Title, true));
                        return true;
                    },
                },
            };

            return resultlist;
        }

        public void UpdateSettings(PowerLauncherSettings settings)
        {
        }
    }
}
