using Microsoft.Win32;
using System.Collections.Generic;

namespace AppSettings
{
    /// <summary>
    /// Extends the base Setting class to handle list of string setting types. Parses a comma-separated
    /// string into a List<string> object
    /// </summary>

    public class StringListSetting : Setting
    {
        public override object SettingVal
        {
            get
            {
                if (SettingValue == null) return null;
                if (SettingValue.Count == 0) return "empty";
                return string.Join(",", SettingValue.ToArray());
            }
        }

        protected List<string> SettingValue = new List<string>();

        public List<string> Value
        {
            get
            {
                return SettingValue;
            }

            set
            {
                SettingValue = value;
            }
        }

        public override bool Accept(string Key, string Value)
        {
            if (Key.ToLower() == SettingKey.ToLower())
            {
                SettingValue = new List<string>(Value.Split(','));
                SettingInitialized = true;
                return true;
            }
            return false;
        }

        public override bool Accept(string Key, Stack<string> CmdLineArgs)
        {
            if (Key.ToLower() == SettingKey.ToLower() && CmdLineArgs.Count > 0)
            {
                return Accept(Key, CmdLineArgs.Pop());
            }
            return false;
        }

        public static implicit operator List<string>(StringListSetting Setting)
        {
            return Setting.SettingValue;
        }

        public StringListSetting(string Key, string ArgValHint, List<string> DefaultValue, ArgTyp ArgType, bool Persist, bool IsInternal, string Help) : base(Key, ArgValHint, ArgType, Persist, IsInternal, Help)
        {
            SettingRegTyp = RegistryValueKind.String;
            if (DefaultValue != null)
            {
                SettingValue = DefaultValue;
                if (ArgType != ArgTyp.Mandatory)
                {
                    SettingInitialized = true;
                }
            }
        }
    }

}
