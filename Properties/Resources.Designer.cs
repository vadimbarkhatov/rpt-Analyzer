﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CHEORptAnalyzer.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CHEORptAnalyzer.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;doc&gt;
        ///  &lt;brackets left=&quot;{&quot; right=&quot;}&quot; left2=&quot;(&quot; right2=&quot;)&quot; /&gt;
        ///  &lt;style name=&quot;Green&quot; color=&quot;Green&quot;/&gt;
        ///  &lt;style name=&quot;Maroon&quot; color=&quot;Maroon&quot;/&gt;
        ///  &lt;style name=&quot;Black&quot; color=&quot;Black&quot;/&gt;
        ///  &lt;style name=&quot;Blue&quot; color=&quot;Blue&quot;/&gt;
        ///  &lt;rule style=&quot;Green&quot;&gt;(\/\/).*&lt;/rule&gt;
        ///  &lt;rule style=&quot;Black&quot;&gt;{(.*?)}&lt;/rule&gt;
        ///  &lt;rule style=&quot;Black&quot;&gt;&quot;(.*?)&quot;&lt;/rule&gt;
        ///  &lt;rule style=&quot;Blue&quot; options=&quot;IgnoreCase&quot;&gt;(in|and|or|if|then|else|like|not)&lt;/rule&gt;
        ///
        ///
        ///&lt;/doc&gt;.
        /// </summary>
        public static string CrystalSyntax {
            get {
                return ResourceManager.GetString("CrystalSyntax", resourceCulture);
            }
        }
    }
}
