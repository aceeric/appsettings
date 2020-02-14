using Microsoft.Win32;
using System.Collections.Generic;

namespace AppSettings
{
    /// <summary>
    /// Extends the base Setting class to handle string setting types
    /// </summary>

    public class StringSetting : Setting
    {
        protected string SettingValue = string.Empty;

        public override object SettingVal { get { return SettingValue; } }

        public string Value
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
                SettingValue = Value;
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

        public static implicit operator string(StringSetting Setting)
        {
            return Setting.SettingValue;
        }

        public StringSetting(string Key, string ArgValHint, string DefaultValue, ArgTyp ArgType, bool Persist, bool IsInternal, string Help) : base(Key, ArgValHint, ArgType, Persist, IsInternal, Help)
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
