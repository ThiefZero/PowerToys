﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ManagedCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using UnitsNet;
using UnitsNet.Units;
using Wox.Plugin;
using System.Diagnostics;

namespace Microsoft.Plugin.UnitConverter
{
    public class Main : IPlugin, /*IPluginI18n,*/ IDisposable
    {
        private PluginInitContext _context;
        private static string _icon_path;
        private bool _disposed;
        private readonly QuantityType[] _included = new QuantityType[] { QuantityType.Acceleration, QuantityType.Length, QuantityType.Mass, QuantityType.Speed, QuantityType.Temperature, QuantityType.Volume };


        public void Init(PluginInitContext context) {
            if (context == null) {
                throw new ArgumentNullException(paramName: nameof(context));
            }

            _context = context;
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(_context.API.GetCurrentTheme());
        }

        public List<Result> Query(Query query) {
            if (query == null) {
                throw new ArgumentNullException(paramName: nameof(query));
            }

            string[] split = query.Search.Split(' ');
            if (split.Length < 4 || split.Length > 4) {
                // deny any other queries than:
                // 10 ft in cm
                // 10 ft to cm
                return new List<Result>();
            }

            string input_first_unit = split[1];//.ToLower();
            string input_second_unit = split[3];//.ToLower();
            double converted = -1;

            List<Result> final_list = new List<Result>();

            foreach (QuantityType quantity_type in _included) {
                QuantityInfo unit_info = Quantity.GetInfo(quantity_type);
                bool first_unit_recognized = UnitParser.Default.TryParse(split[1], unit_info.UnitType, out Enum first_unit);
                bool second_unit_recognized = UnitParser.Default.TryParse(split[3], unit_info.UnitType, out Enum _);

                // 3 types of matches:
                // a) 10 ft in cm (double abbreviation)
                // b) 10 feet in centimeter (double unabbreviated)
                // c) 10 feet in cm (single abbreviation

                if (first_unit_recognized && second_unit_recognized) {
                    // a
                    converted = UnitsNet.UnitConverter.ConvertByAbbreviation(int.Parse(split[0]), unit_info.Name, input_first_unit, input_second_unit);
                    AddToResult(final_list, converted, split[3]);
                }
                else if ((first_unit_recognized && !second_unit_recognized) || (!first_unit_recognized && second_unit_recognized)) {
                    // c
                    if (first_unit_recognized) {
                        // get first unabbreviated, convert first to second
                    }
                    else if (second_unit_recognized) {
                        // get second unabbreviated, convert first to second
                    }
                }
                else if ((!first_unit_recognized) && (!second_unit_recognized)) {
                    // b
                    bool first_unabbreviated = Array.Exists(unit_info.UnitInfos, unitName => unitName.Name.ToLower() == input_first_unit);
                    bool second_unabbreviated = Array.Exists(unit_info.UnitInfos, unitName => unitName.Name.ToLower() == input_second_unit);

                    if (first_unabbreviated && second_unabbreviated) {
                        converted = UnitsNet.UnitConverter.ConvertByName(double.Parse(split[0]), unit_info.Name, input_first_unit, input_second_unit);
                        AddToResult(final_list, converted, split[3]);
                    }
                }
            }

            return final_list;
        }

        private void AddToResult(List<Result> currentList, double converted_value, string unit_name) {
            // answer found, add result to list
            currentList.Add(new Result {
                Title = string.Format("{0} {1}", converted_value, unit_name),
                IcoPath = _icon_path,
                Score = 300,
                SubTitle = "Copy to clipboard", //Context.API.GetTranslation("wox_plugin_calculator_copy_number_to_clipboard"),
                Action = c => {
                    var ret = false;
                    var thread = new Thread(() => {
                        try {
                            Clipboard.SetText(converted_value.ToString());
                            ret = true;
                        }
                        catch (ExternalException) {
                            MessageBox.Show("Copy failed, please try later");
                        }
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                    return ret;
                }
            });
        }

        private void OnThemeChanged(Theme _, Theme newTheme) {
            UpdateIconPath(newTheme);
        }

        private static void UpdateIconPath(Theme theme) {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite) {
                _icon_path = "Images/Warning.light.png";
            }
            else {
                _icon_path = "Images/Warning.dark.png";
            }
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    _context.API.ThemeChanged -= OnThemeChanged;
                    _disposed = true;
                }
            }
        }
    }
}
