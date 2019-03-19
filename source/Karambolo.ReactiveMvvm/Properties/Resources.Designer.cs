﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Karambolo.ReactiveMvvm.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Karambolo.ReactiveMvvm.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to None of the registered {0} implementations are able to observe changes of the data member {{MEMBER}} (BeforeChange={{BEFORE}})..
        /// </summary>
        internal static string ChangeNotificationNotPossible {
            get {
                return ResourceManager.GetString("ChangeNotificationNotPossible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please make sure that your platform is registered at application startup in {0}.{1}..
        /// </summary>
        internal static string CheckIfPlatformRegistered {
            get {
                return ResourceManager.GetString("CheckIfPlatformRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to None of the registered {0} implementations are able to establish the command binding {{SOURCEPATH}} -&gt; {{TARGETPATH}}..
        /// </summary>
        internal static string CommandBindingNotPossible {
            get {
                return ResourceManager.GetString("CommandBindingNotPossible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Constant source expressions are not supported..
        /// </summary>
        internal static string ConstantSourceExpression {
            get {
                return ResourceManager.GetString("ConstantSourceExpression", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Assignment of value &apos;{VALUE}&apos; of type {TYPE} failed on binding {SOURCEPATH} -&gt; {TARGETPATH}..
        /// </summary>
        internal static string DataBindingAssignmentFailed {
            get {
                return ResourceManager.GetString("DataBindingAssignmentFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Conversion of value &apos;{VALUE}&apos; from type {FROMTYPE} to type {TOTYPE} failed on binding {SOURCEPATH} -&gt; {TARGETPATH}..
        /// </summary>
        internal static string DataBindingConversionFailed {
            get {
                return ResourceManager.GetString("DataBindingConversionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Chain is incompatible with the specified source and target types..
        /// </summary>
        internal static string IncompatibleChain {
            get {
                return ResourceManager.GetString("IncompatibleChain", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Command accepting parameters of type {0} was invoked with an incompatible parameter value of type {1}..
        /// </summary>
        internal static string IncompatibleCommandParamType {
            get {
                return ResourceManager.GetString("IncompatibleCommandParamType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Service provider is already in use, thus initialization is not possible any more..
        /// </summary>
        internal static string InitializationNotPossible {
            get {
                return ResourceManager.GetString("InitializationNotPossible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Property expression is invalid. It must be specified like &apos;obj =&gt; obj.Property&apos;..
        /// </summary>
        internal static string InvalidPropertyExpression {
            get {
                return ResourceManager.GetString("InvalidPropertyExpression", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Non-constant index expressions are not supported..
        /// </summary>
        internal static string NonConstantIndexExpression {
            get {
                return ResourceManager.GetString("NonConstantIndexExpression", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Command accepting parameters of non-nullable type {0} was invoked with null..
        /// </summary>
        internal static string NonNullableCommandParamType {
            get {
                return ResourceManager.GetString("NonNullableCommandParamType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ReactiveMvvm is not fully functional because the platform-specific components were not registered. You need to install the ReactiveMvvm.&lt;Platform&gt; package matching your application platform and register it at application startup using {0}.{1}..
        /// </summary>
        internal static string PlatformNotRegistered {
            get {
                return ResourceManager.GetString("PlatformNotRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error was encountered while consuming an observable sequence. To prevent this, ensure the sequence does not produce an error or define error filtering for the consuming component by specifying a custom {0}..
        /// </summary>
        internal static string UnhandledObservedError {
            get {
                return ResourceManager.GetString("UnhandledObservedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Expression type {0} is not supported..
        /// </summary>
        internal static string UnsupportedExpressionType {
            get {
                return ResourceManager.GetString("UnsupportedExpressionType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to None of the registered {0} implementations are able to install view activation for views of type {{TYPE}}..
        /// </summary>
        internal static string ViewActivationNotPossible {
            get {
                return ResourceManager.GetString("ViewActivationNotPossible", resourceCulture);
            }
        }
    }
}
