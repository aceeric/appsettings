using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace AppSettings
{
    /// <summary>
    /// Extends the base Setting class to handle date/time setting types.
    /// </summary>

    public class DateTimeSetting : Setting
    {
        public override object SettingVal { get { return SettingValue; } }

        protected DateTime SettingValue;

        public DateTime Value
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
                SettingValue = Value == string.Empty ? SettingValue : DateTime.Parse(Value);
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

        public static implicit operator DateTime(DateTimeSetting Setting)
        {
            return Setting.SettingValue;
        }

        public DateTimeSetting(string Key, string ArgValHint, DateTime DefaultValue, ArgTyp ArgType, bool Persist, bool IsInternal, string Help) : base(Key, ArgValHint, ArgType, Persist, IsInternal, Help)
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
