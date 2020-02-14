using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AppSettings
{
    /// <summary>
    /// Provides worker functionality for derived Settings classes. Consumers must subclass
    /// and define usage, specific settings, and potentially provide custom setting handling.
    /// </summary>

    public abstract class AppSettingsBase
    {
        /// <summary>
        /// Defines the registry key that will be used to store settings in the registry. Must be defined in the subclass.
        /// </summary>
        /// <returns>A registry key within HKLM\SOFTWARE. E.g. a return value of "MyCompany\MyProduct\v1"</returns>
        /// would find this registry key: "HKLM\SOFTWARE\MyCompany\MyProduct\v1"

        public static string RegKey { get; }

        /// <summary>
        /// If there is an error parsing the settings, then the module will populate this field, which the consumer can
        /// query and provide to the caller, if appropriate given the runtime context.
        /// </summary>

        public static string ParseErrorMessage { get; set; }

        /// <summary>
        /// Parses the passed source for settings. Override to add custom parsing (i.e. valid values, ranges, etc)
        /// </summary>
        /// <param name="Src"></param>
        /// <param name="CmdLineArgs"></param>
        /// <remarks>
        /// If the settings were previously parsed from another source then the second parse operation overwrites
        /// the first for settings that are defined in both sources.
        /// </remarks>
        /// <returns></returns>

        public static bool Parse(SettingsSource Src, string[] CmdLineArgs = null)
        {
            switch (Src)
            {
                case SettingsSource.Registry:
                    if (!ParseRegistry())
                    {
                        return false;
                    }
                    break;
                case SettingsSource.SettingsFile:
                    if (!ParseSettingsFile())
                    {
                        return false;
                    }
                    break;
                case SettingsSource.CommandLine:
                    if (!ParseCommandLine(CmdLineArgs))
                    {
                        return false;
                    }
                    break;
            }

            foreach (Setting s in SettingsDict.Values)
            {
                if (s.ArgType == Setting.ArgTyp.StopIfProvided && s.Initialized)
                {
                    return true;
                }
                if (s.ArgType == Setting.ArgTyp.Mandatory && !s.Initialized)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Saves the settings in their current state to the passed source.
        /// </summary>
        /// <param name="Src">Supported sources. See the SettingsSource enum</param>

        public static void Save(SettingsSource Src)
        {
            switch (Src)
            {
                case SettingsSource.Registry:
                    SaveRegistrySettings();
                    break;
                case SettingsSource.SettingsFile:
                    SaveFileSettings();
                    break;
                default:
                    throw new ParseException(string.Format("Unsupport Operation: Attempt to save an unsupported settings source: {0}", Src));
            }
        }

        /// <summary>
        /// Saves application settings to the settings file defined for the application
        /// </summary>

        private static void SaveFileSettings()
        {
            using (StreamWriter Sw = new StreamWriter(SettingsFileName))
            {
                foreach (Setting s in SettingsDict.Values)
                {
                    if (s.Persist)
                    {
                        Sw.WriteLine(string.Format("{0}={1}", s.Key, s.SettingVal == null ? string.Empty : s.SettingVal));
                    }
                }
            }
        }

        /// <summary>
        /// Saves application settings to the registry
        /// </summary>

        private static void SaveRegistrySettings()
        {
            using (RegistryKey k = OpenRegistry(true))
            {
                foreach (Setting s in SettingsDict.Values)
                {
                    if (s.Persist)
                    {
                        if (s.RegTyp == RegistryValueKind.MultiString)
                        {
                            throw new ParseException("Unsupported registry key format: REG_MULIT_SZ");
                        }
                        else
                        {
                            k.SetValue(s.Key, s.SettingVal == null ? string.Empty : s.SettingVal.ToString(), s.RegTyp);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes the settings and their state to the console. This is a testing method
        /// </summary>

        public static void DisplayToConsole()
        {
            foreach (Setting s in SettingsDict.Values)
            {
                Console.WriteLine(s);
            }
        }

        /// <summary>
        /// Prints usage instructions to the console. Prints the synopsis, follwed by a command line exemplar,
        /// followed by help for each argument
        /// </summary>

        public static void ShowUsage()
        {
            Assembly Asm = Assembly.GetEntryAssembly();
            AssemblyName AsmName = Asm.GetName();
            string CmdLine = AsmName.Name;
            string Copyright = string.Empty;
            foreach (object o in Asm.GetCustomAttributes(false))
            {
                if (o.GetType() == typeof(System.Reflection.AssemblyCopyrightAttribute))
                {
                    AssemblyCopyrightAttribute Aca = (AssemblyCopyrightAttribute)o;
                    Copyright = Aca.Copyright;
                }
            }

            string Version = string.Format("\nVersion {0}.{1}.{2}.{3} {4}",
                AsmName.Version.Major, AsmName.Version.Minor, AsmName.Version.Build, AsmName.Version.Revision, Copyright).TrimEnd();
            Console.WriteLine("\n" +CmdLine.ToUpper() + "\n" + CmdLine.Stuff("=") + Version);

            Console.WriteLine("\n" + string.Join("\n", BreakLongString(Usage).ToArray()) + "\n");

            // This loop prints the command along with each supported setting/arg in a bracket along with the
            // settiong/arg's value hint. E.g.: "thiscommand [-timeout 100] [-flag] [-key XYZ] etc..."
            // while doing this is calculates a max length of each command and hint so that in the next
            // loop when it shows the help, it can indent the help text at a uniform location

            int MaxArgLen = 0;
            foreach (Setting s in SettingsDict.Values)
            {
                if (!s.IsInternal)
                {
                    string Arg = string.Format("-{0}{1}{2}", s.Key.ToLower(), s.ArgValHint != null ? " " : string.Empty, s.ArgValHint);
                    MaxArgLen = MaxArgLen > Arg.Length ? MaxArgLen : Arg.Length;
                    CmdLine += string.Format(" [{0}]", Arg);
                }
            }
            Console.WriteLine("Usage:\n\n" + CmdLine + "\n");
            foreach (Setting s in SettingsDict.Values)
            {
                if (!s.IsInternal) // don't show help on internal settings/args
                {
                    string Arg = string.Format("-{0}{1}{2}", s.Key.ToLower(), s.ArgValHint != null ? " " : string.Empty, s.ArgValHint);
                    Arg += Spacer(Arg, MaxArgLen);
                    Console.Write(Arg);
                    List<string> Help = BreakLongString(XlatArgType(s.ArgType) + ". " + s.ArgHelp, Console.WindowWidth - Arg.Length - 1);
                    for (int i = 0; i < Help.Count; ++i)
                    {
                        Console.WriteLine((i == 0 ? string.Empty : Spacer(string.Empty, MaxArgLen)) + Help[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Translates the Setting.ArgTyp.StopIfProvided to Setting.ArgTyp.Optional
        /// </summary>
        /// <param name="Typ"></param>
        /// <returns></returns>

        private static Setting.ArgTyp XlatArgType(Setting.ArgTyp Typ)
        {
            return Typ == Setting.ArgTyp.StopIfProvided ? Setting.ArgTyp.Optional : Typ;
        }

        /// <summary>
        /// Break up the passed string into sub-strings. If not Len argument is passed, then fit to the console
        /// window width, otherwise fit to the passed Len
        /// </summary>
        /// <param name="Str">A string to break into substrings</param>
        /// <param name="Len">A defined length. If passed as <= 0 then the console width is used.</param>
        /// <returns>A list of strings of approximately Len length</returns>

        private static List<string> BreakLongString(string Str, int Len = 0)
        {
            Len = Len <= 0 ? Console.WindowWidth - 1: Len;

            List<string> Parts = new List<string>();

            int Cur = 0;
            for (int End = Len; End < Str.Length; Cur = End + 1, End = Cur + Len)
            {
                int Seek = End;
                while (Str[Seek] != ' ' && Seek > Cur)
                {
                    --Seek;
                }
                if (Seek > Cur) // otherwise there are no spaces so the string will just be chopped
                {
                    End = Seek;
                }
                Parts.Add(Str.Substring(Cur, End - Cur));
            }
            if (Cur < Str.Length) // could be exactly on the button
            {
                Parts.Add(Str.Substring(Cur));
            }
            return Parts;
        }

        /// <summary>
        /// Parses the command line arguments and passes them to the settings. If all arguments are accepted then returns true, else returns false
        /// </summary>
        /// <param name="args">Array of arguments as recevied by the Main function</param>
        /// <returns>True if all required settings were defined on the command line and no unknown settings were defined on the command line</returns>

        private static bool ParseCommandLine(string[] args)
        {
            Stack<string> Stk = new Stack<string>();
            for (int i = args.Length - 1; i >= 0; i--)
            {
                Stk.Push(args[i]); // a stack works nicely for passing the args around to the settings
            }
            while (Stk.Count > 0)
            {
                string Key = Stk.Pop().Substring(1); // args are in the form "-foo" but the keys are like "foo"

                bool Accepted = false;
                foreach (Setting s in SettingsDict.Values)
                {
                    if (s.Accept(Key, Stk))
                    {
                        // if the setting accepts the key, and there is also a value (e.g. -timeout 100), the setting will have
                        // also popped the value off the stack, so the top of the stack will be another key on return from Accept
                        Accepted = true;
                        if (s.ArgType == Setting.ArgTyp.StopIfProvided)
                        {
                            return true;
                        }
                        break;
                    }
                }
                if (!Accepted) // then we got an arg that no setting understands
                {
                    ParseErrorMessage = "Unknown arg: -" + Key;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Generates a filename of a settings file. Can be overridden to provide a different file name or location
        /// </summary>
        /// <returns></returns>

        public static string SettingsFileName
        {
            get
            {
                string EXEName = "UNKNOWN";
                try
                {
                    EXEName = Assembly.GetEntryAssembly().GetName().Name;
                }
                catch { }
                return Path.Combine(Environment.CurrentDirectory, EXEName + ".settings");
            }
        }

        /// <summary>
        /// Opens the registry
        /// </summary>
        /// <returns></returns>

        private static RegistryKey OpenRegistry(bool writable = false)
        {
            string SettingsKey = "SOFTWARE\\" + (RegKey[0] == '\\' ? RegKey.Substring(1) : RegKey);
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(SettingsKey, writable);
        }

        /// <summary>
        /// Retrieves all the values defined for the application's registry key and attempts to assign those values
        /// to the settings
        /// </summary>
        /// <returns>True if all required settings were defined in the registry and no unknown settings were defined in the registry</returns>

        private static bool ParseRegistry()
        {
            Hashtable Nv = new Hashtable();

            using (RegistryKey k = OpenRegistry())
            {
                if (k == null)
                {
                    throw new ParseException(string.Format("Fatal Error: Registry key {0} does not exist", RegKey));
                }
                foreach (string ValName in k.GetValueNames())
                {
                    if (ValName == string.Empty)
                    {
                        continue; // ignore empty setting keys
                    }
                    object Val = k.GetValue(ValName);
                    string StrVal = null;
                    if (k.GetValueKind(ValName) == RegistryValueKind.MultiString)
                    {
                        throw new ParseException("Unsupported registry key format: REG_MULIT_SZ");
                    }
                    else
                    {
                        StrVal = Val.ToString();
                    }
                    Nv.Add(ValName, StrVal);
                }
            }
            return ParseNvList(Nv);
        }

        /// <summary>
        /// Parses settings from a settings file. The settings file is located in the same directory as the executable and is named
        /// exename.settings. The format of the file must be one setting per line in the form: name=value\n
        /// </summary>
        /// <returns>True if all required settings were defined in the file and no unknown settings were defined in the file</returns>

        private static bool ParseSettingsFile()
        {
            Hashtable Nv = new Hashtable();
            string[] Lines = null;

            if (!File.Exists(SettingsFileName))
            {
                throw new ParseException(string.Format("Fatal Error: Settings file {0} does not exist in the application working folder", SettingsFileName));
            }
            Lines = File.ReadAllLines(SettingsFileName);

            foreach (string Line in Lines)
            {
                string[] Parts = Line.SplitFirst('=');
                if (Parts.Length == 1)
                {
                    Nv.Add(Parts[0], null);
                }
                else
                {
                    Nv.Add(Parts[0], Parts[1]);
                }
            }
            return ParseNvList(Nv);
        }

        /// <summary>
        /// Passess the name/value list to each setting to see if that setting will accept the N/V pair. If a setting
        /// cannot be found to accept the pair, then that is an error - it means the settings contain invalid assertions
        /// </summary>
        /// <param name="Nv">Hashtable of Name/Value pairs</param>
        /// <returns>true if all NVP items were accepted by a setting</returns>

        private static bool ParseNvList(Hashtable Nv)
        {
            foreach (string Key in Nv.Keys)
            {
                bool Accepted = false;
                foreach (Setting s in SettingsDict.Values)
                {
                    if (s.Accept(Key, Nv[Key] == null ? string.Empty : (string)Nv[Key]))
                    {
                        Accepted = true;
                        if (s.ArgType == Setting.ArgTyp.StopIfProvided)
                        {
                            return true;
                        }
                        break;
                    }
                }
                if (!Accepted) // then we got an arg that nobody understands
                {
                    ParseErrorMessage = "Unknown setting: " + Key;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a string that, if appended to Arg, would result in a string of legth Max. Used to
        /// make a series of strings have identical lenghs for formatting purposes.
        /// </summary>
        /// <example>
        /// string s = Spacer("hello", 10);
        /// Console.WriteLine("*" + s + "*");
        /// // result would be: *hello     *
        /// </example>
        /// <param name="Arg">A string</param>
        /// <param name="Max">The length to space Arg out to. The method adds 2 to this to buffer the string.</param>
        /// <returns>The modified string</returns>

        private static string Spacer(string Arg, int Max)
        {
            return new string(' ', Max - Arg.Length + 2);
        }

        /// <summary>
        /// Assigns the passed array of Setting objects to this object.
        /// </summary>

        protected static Setting[] SettingList
        {
            set
            {
                foreach (Setting s in value)
                {
                    SettingsDict.Add(s.Key, s);
                }
            }
        }

        /// <summary>
        /// Holds the synopsis
        /// </summary>

        protected static string Usage { get; set; }

        /// <summary>
        /// Case-insensitive dictionary that holds the Setting objects that were assigned via the SettingList method
        /// </summary>

        protected static Dictionary<string, Setting> SettingsDict = new Dictionary<string, Setting>(StringComparer.CurrentCultureIgnoreCase);
    }
}
