﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Community.PowerToys.Run.Plugin.UnitConverter.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Community.PowerToys.Run.Plugin.UnitConverter.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Copy (Enter).
        /// </summary>
        public static string context_menu_copy {
            get {
                return ResourceManager.GetString("context_menu_copy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Copy failed.
        /// </summary>
        public static string copy_failed {
            get {
                return ResourceManager.GetString("copy_failed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Copy {0} to clipboard.
        /// </summary>
        public static string copy_to_clipboard {
            get {
                return ResourceManager.GetString("copy_to_clipboard", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provides unit conversion..
        /// </summary>
        public static string plugin_description {
            get {
                return ResourceManager.GetString("plugin_description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unit Converter.
        /// </summary>
        public static string plugin_name {
            get {
                return ResourceManager.GetString("plugin_name", resourceCulture);
            }
        }
    }
}
