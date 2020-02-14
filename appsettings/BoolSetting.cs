using Microsoft.Win32;
using System.Collections.Generic;

namespace AppSettings
{
    /// <summary>
    /// Extends the base Setting class to handle boolean setting types.
    /// </summary>

    public class BoolSetting : Setting
    {
        public override object SettingVal { get { return SettingValue; } }

        public class MyBool
        {
            public bool val;
            public MyBool(bool b) { val = b; }
            public static implicit operator bool(MyBool b)
            {
                return b.val;
            }
            public static implicit operator MyBool(bool b)
            {
                return new MyBool(b);
            }
        }

        protected bool SettingValue;

        public bool Value
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
                // Accept can be called when parsing command line args and when parsing a settings store. If parsing a settings
                // store we might get a boolean as name=value. If from the command line, Value will be string.Empty
                if (Value == string.Empty)
                {
                    SettingValue = true;
                }
                else
                {
                    SettingValue = bool.Parse(Value);
                }
                SettingInitialized = true;
                return true;
            }
            return false;
        }

        public override bool Accept(string Key, Stack<string> CmdLineArgs)
        {
            if (Key.ToLower() == SettingKey.ToLower())
            {
                return Accept(Key, string.Empty);
            }
            return false;
        }

        public static implicit operator bool(BoolSetting Setting)
        {
            return Setting.SettingValue;
        }

        public BoolSetting(string Key, MyBool DefaultValue, ArgTyp ArgType, bool Persist, bool IsInternal, string Help) : base(Key, null, ArgType, Persist, IsInternal, Help)
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
