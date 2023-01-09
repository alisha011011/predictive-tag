﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Predictive.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Predictive.Properties.Resources", typeof(Resources).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;root datatype=&quot;config&quot; comment=&quot;Default config file&quot;&gt;
        ///  &lt;language&gt;
        ///    &lt;spellchecker&gt;en-US&lt;/spellchecker&gt;
        ///  &lt;/language&gt;
        ///  &lt;presets&gt;
        ///    &lt;preset text=&quot;Add person tag&quot;&gt;
        ///      &lt;tag&gt;person&lt;/tag&gt;
        ///    &lt;/preset&gt;
        ///    &lt;preset text=&quot;Add people tag&quot;&gt;
        ///      &lt;tag&gt;people&lt;/tag&gt;
        ///    &lt;/preset&gt;
        ///    &lt;preset text=&quot;&quot;&gt;
        ///      &lt;tag&gt;&lt;/tag&gt;
        ///      &lt;tag&gt;&lt;/tag&gt;
        ///    &lt;/preset&gt;
        ///    &lt;preset text=&quot;&quot;&gt;
        ///      &lt;tag&gt;&lt;/tag&gt;
        ///      &lt;tag&gt;&lt;/tag&gt;
        ///    &lt;/preset&gt;
        ///    &lt;preset text=&quot;&quot;&gt;
        ///      &lt;tag [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string config {
            get {
                return ResourceManager.GetString("config", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] delete {
            get {
                object obj = ResourceManager.GetObject("delete", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] folder {
            get {
                object obj = ResourceManager.GetObject("folder", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // DO NOT REMOVE THIS LINE [datatype=&quot;historical-data&quot;]
        ///// Demo historical data file (minimal).
        ///
        ///animal, bird, fauna, owl, species, wildlife
        ///fog, forest, nature, trees, wilderness, woods
        ///animal, fauna, reptile, snake, species, wildlife
        ///asphalt, autumn, fall, forest, highway, road, trees
        ///boats, dusk, lake, outdoors, sunset, travel, vacation
        ///beach, dusk, nature, outdoors, sea, sunset, travel, vacation
        ///child, hat, market, shawl, woman
        ///child, childhood, flowers, girl, grass, meadow
        ///animal, elephant,  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string historical_data {
            get {
                return ResourceManager.GetString("historical_data", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] icon {
            get {
                object obj = ResourceManager.GetObject("icon", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;root datatype=&quot;learn-data&quot; comment=&quot;Default learn data file&quot;&gt;
        ///  &lt;item tag=&quot;acrylic&quot; suggest=&quot;art&quot; score=&quot;2&quot; /&gt;
        ///  &lt;item tag=&quot;animal&quot; suggest=&quot;species&quot; score=&quot;1&quot; /&gt;
        ///  &lt;item tag=&quot;bird&quot; suggest=&quot;wildlife&quot; score=&quot;1&quot; /&gt;
        ///  &lt;item tag=&quot;elephant&quot; suggest=&quot;animal&quot; score=&quot;1&quot; /&gt;
        ///  &lt;item tag=&quot;giraffe&quot; suggest=&quot;animal&quot; score=&quot;1&quot; /&gt;
        ///  &lt;item tag=&quot;grapes&quot; suggest=&quot;fruit&quot; score=&quot;1&quot; /&gt;
        ///  &lt;item tag=&quot;lemon&quot; suggest=&quot;citrus&quot; score=&quot;1&quot; /&gt;
        ///  &lt;item tag=&quot;newt&quot; suggest=&quot;reptile&quot; score=&quot; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string learn_data {
            get {
                return ResourceManager.GetString("learn_data", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // DO NOT REMOVE THIS LINE [datatype=&quot;notify-data&quot;]
        ///// Demo notify data file (empty)
        ///
        ///
        ///.
        /// </summary>
        internal static string notify_data {
            get {
                return ResourceManager.GetString("notify_data", resourceCulture);
            }
        }
    }
}