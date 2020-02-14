using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace AppSettings
{
    /// <summary>
    /// Extends the base Setting class to handle integer setting types
    /// </summary>

    public class IntSetting : Setting
    {
        public override object SettingVal { get { return SettingValue; } }

        public class MyInt
        {
            public int val;
            public MyInt(int i) { val = i; }
            public static implicit operator int(MyInt i)
            {
                return i.val;
            }
            public static implicit operator MyInt(int i)
            {
                return new MyInt(i);
            }
        }

        protected int SettingValue = 0;

        public int Value
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
                SettingValue = Value == string.Empty ? SettingValue : Int32.Parse(Value);
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

        public static implicit operator int(IntSetting Setting)
        {
            return Setting.SettingValue;
        }

        public IntSetting(string Key, string ArgValHint, MyInt DefaultValue, ArgTyp ArgType, bool Persist, bool IsInternal, string Help) : base(Key, ArgValHint, ArgType, Persist, IsInternal, Help)
        {
            SettingRegTyp = RegistryValueKind.DWord;
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
