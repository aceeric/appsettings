using Microsoft.Win32;
using System.Collections.Generic;

namespace AppSettings
{
    /// <summary>
    /// Defines the basic behavior of a configuration setting
    /// </summary>

    public abstract class Setting
    {
        /// <summary>
        /// Defines processing flags for settings / command-line args
        /// </summary>

        public enum ArgTyp
        {
            /// <summary>
            /// Mandatory setting or command-line argument. Must be specified in a settings file, the registry, or the command line
            /// or the Parse method will return false
            /// </summary>

            Mandatory,

            /// <summary>
            /// Optional setting or command-line argument
            /// </summary>

            Optional,

            /// <summary>
            /// Only relevant for command-line arg parsing. If this arg is encountered
            /// then the command-line parser will stop processing the command line and return. Typically
            /// used to provide a command-line argument that specifies that settings are to be obtained
            /// from a settings file or the registry
            /// </summary>

            StopIfProvided
        }

        /// <summary>
        /// Gets the key associated with the setting
        /// </summary>
        public string Key { get { return SettingKey; } }

        /// <summary>
        /// Returns true if the setting was initialized (as in from the command line or a settings file) else
        /// returns false.
        /// </summary>
        public bool Initialized { get { return SettingInitialized; } }

        /// <summary>
        /// Returns the type of the setting: optional, mandatory, etc. See ArgType
        /// </summary>
        public ArgTyp ArgType { get { return SettingArgType; } }

        /// <summary>
        /// Returns a hint to how the arg value might be formatted or specified
        /// </summary>
        public string ArgValHint { get { return SettingArgValHint; } }

        /// <summary>
        /// Provides the detailed instructions on the purpose of the argument/setting
        /// </summary>
        public string ArgHelp { get { return SettingArgHelp; } }

        /// <summary>
        /// Returns true if the setting is to be persisted (as in to a settings file, or the registry) upon
        /// program termination
        /// </summary>
        public bool Persist { get { return SettingPersist; } }

        /// <summary>
        /// Gets the windows registry data type for the setting. See RegistryValueKind
        /// </summary>
        public RegistryValueKind RegTyp { get { return SettingRegTyp; } }

        /// <summary>
        /// Returns true if the setting is defined as internal, else returns false. The intent of internal
        /// settings is the usage instructions are not displayed for internal settings. These might be settings
        /// that an administrator might provide on the command line that the general user populated should not
        /// be made aware of
        /// </summary>
        public bool IsInternal { get { return SettingIsInternal;  } }

        /// <summary>
        /// Initializes an instance with the passed arguments
        /// </summary>
        /// <param name="Key">The key for this arg/setting</param>
        /// <param name="ArgValHint">A hint as to what the arg/setting value might be. Should be elaborated in the Help arg</param>
        /// <param name="ArgType">See the ArgType enum</param>
        /// <param name="Persist">True if the arg/setting should be persisted when Save is called</param>
        /// <param name="IsInternal">True if this arg/setting should not be displyed in the usage instructions</param>
        /// <param name="Help">Instructions on how to code the arg/setting</param>

        protected Setting(string Key, string ArgValHint, ArgTyp ArgType, bool Persist, bool IsInternal, string Help)
        {
            SettingKey = Key;
            SettingArgValHint = ArgValHint;
            SettingArgType = ArgType;
            SettingPersist = Persist;
            SettingIsInternal = IsInternal;
            SettingArgHelp = Help;
        }

        /// <summary>
        /// Returns the instance as a string with key elements
        /// </summary>
        /// <returns></returns>

        public override string ToString()
        {
            return string.Format("{0}: {1}; {2}; Value: {3}",
                Key,
                SettingArgType,
                Initialized ? "Initialized" : "Uninitialized",
                SettingVal);
        }

        /// <summary>
        /// Provides a representation of the setting.
        /// </summary>

        public abstract object SettingVal { get; }

        /// <summary>
        /// Called when parsing args from the registry or a settings file. Compares the passed key
        /// to the setting's key and if it matches, consumes the value and returns true. Otherwise, returns false.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>

        public abstract bool Accept(string Key, string Value);

        /// <summary>
        /// Called when parsing args from the Console command line. Compares the passed key
        /// to the setting's key and if it matches, consumes the value and returns true. Otherwise, returns false.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>

        public abstract bool Accept(string Key, Stack<string> CmdLineArgs);

        // This is the private state of the class

        protected string SettingKey = null;
        protected bool SettingInitialized = false;
        protected ArgTyp SettingArgType = ArgTyp.Optional;
        protected string SettingArgValHint = null;
        protected string SettingArgHelp = null;
        protected bool SettingPersist = false;
        protected RegistryValueKind SettingRegTyp;
        protected bool SettingIsInternal;
    }

}
