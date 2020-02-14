using System;
using System.Collections.Generic;
using System.IO;
using AppSettings;

namespace UnitTest
{
    class Cfg : AppSettingsBase
    {
        public static StringSetting Settings { get { return (StringSetting)SettingsDict["Settings"]; } }
        public static StringListSetting DUNS { get { return (StringListSetting)SettingsDict["DUNS"]; } }
        public static IntSetting BatchSize { get { return (IntSetting)SettingsDict["BatchSize"]; } }
        public static BoolSetting InitSettings { get { return (BoolSetting)SettingsDict["InitSettings"]; } }
        public static BoolSetting Foo { get { return (BoolSetting)SettingsDict["Foo"]; } }
        public static StringSetting Frobazz { get { return (StringSetting)SettingsDict["Frobazz"]; } }
        public static DateTimeSetting ADateSetting { get { return (DateTimeSetting)SettingsDict["ADateSetting"]; } }
        public static BoolSetting Hidden { get { return (BoolSetting)SettingsDict["Hidden"]; } }

        public new static string RegKey { get { return "SCSInc\\SAMApi\\v1"; } }

        /// <summary>
        /// Initializes the instance with an array of settings that the application supports, as well as usage instructions
        /// </summary>

        static Cfg()
        {
            SettingList = new Setting[] {
                new StringSetting("Settings", "file|reg", null,  Setting.ArgTyp.StopIfProvided, false, false,
                    "Indicates that the utility should obtain configuration information from a settings file or the registry instead of the command line. " +
                    "Specify either 'file', or 'reg'. If this arg is supplied, then all *subsequent* command line arguments are ignored and the settings  " +
                    "are obtained from the settings file or the registry as indicated. If the same settings are specified in the settings file and the " +
                    "command line then the settings file will overwrite the command line setting, otherwise the command line value " +
                    "will remain as provided."),
                new DUNSListSetting("DUNS", "list|@list", null, Setting.ArgTyp.Optional, false, false,
                    "To support testing, provide a comma-separated list of DUNS numbers, or a list of DUNS numbers in a file. " +
                    "If the list specifier is in the form nnn,nnn,nnn... then it is interpreted as a comma-separated list of DUNS " +
                    "numbers. If the list specifier begins with the at sign (@) then the at sign is removed and the remainder of the " +
                    "list specifier is interpreted as a filename. The file is opened and the DUNS list is built from the file. The file " +
                    "can contain multiple lines, with multiple DUNS numbers per line, separated by commas. Under this scenario, " +
                    "the utility does not query the database for the DUNS list. It uses the DUNS numbers provided and then exits when " +
                    "all numbers have been passed to the API."),
                new IntSetting("BatchSize", "n", 100, Setting.ArgTyp.Optional, true, false,
                    "Specifies n as the batch size. The utility will invoke the API no more than n times each time " +
                    "it is run. If the utility is scheduled as an unattended task, then at most n calls to the API " +
                    "will be made on each invocation. If not provided, 100 is used as the default value."),
                new BoolSetting("InitSettings", false,  Setting.ArgTyp.Optional, false, false,
                    "Initializes persistent settings to default values and then exits."),
                new BoolSetting("Foo", false,  Setting.ArgTyp.Mandatory, true, false,
                    "For his part, Mr. Trump has shown no willingness to accept the intelligence community’s views on allegations of Russian hacking. He " +
                    "repeatedly has said the campaign could have been carried out by the Chinese, and once he said it could have been a person in New " +
                    "Jersey. When reports first surfaced more than a week ago that CIA officials believed the Russians were trying to help Mr. Trump, his " +
                    "transition team published a short statement that took aim that the intelligence agency’s credibility. "),
                new StringSetting("Frobazz", "xyz", string.Empty,  Setting.ArgTyp.Optional, true, false,
                    "U.S. intelligence officials in October reached an assessment that the Russian government launched a cyber operation to steal emails " +
                    "and other documents from Democratic Party officials and then selectively leak thousands of records to interfere with the presidential " +
                    "election. Mr. Trump and his top aides have dismissed Russian involvement in the email leaks, and Mr. Trump has reacted angrily to " +
                    "suggestions that the Russians wanted to help him win. Mr. Priebus, though, said Mr. Trump wanted to hear from the FBI before he " +
                    "settled his opinion on what transpired."),
                new DateTimeSetting("ADateSetting", "d", DateTime.Now, Setting.ArgTyp.Mandatory, true, false,
                    "Russian officials have denied being behind the attacks, though Russian President Vladimir Putin has said whoever carried out the " +
                    "campaign conducted a public service by exposing internal Democratic Party emails. " +
                    "The Office of the Director of National Intelligence is preparing a report on the campaign and is expected to make some of its findings " +
                    "public before Mr. Obama’s term ends on Jan. 20, 2017. " +
                    "Senate Armed Services Committee Chairman John McCain (R., Ariz.) has called for the creation of a “select” investigatory committee in " +
                    "Congress to probe the alleged Russian hacking campaign, but GOP leaders have said they believe any investigation should be carried " +
                    "out by existing committees."),
                new BoolSetting("Hidden", false,  Setting.ArgTyp.Optional, false, true,
                    "This is an internal setting that does not display help on the command line."),
            };
            Usage =
                "The capacity of a Dictionary<TKey, TValue> is the number of elements the Dictionary<TKey, TValue> can hold. As elements " +
                "are added to a Dictionary<TKey, TValue>, the capacity is automatically increased as required by reallocating the internal array. " +
                "For very large Dictionary<TKey, TValue> objects, you can increase the maximum capacity to 2 billion elements on a 64-bit " +
                "system by setting the enabled attribute of the configuration element to true in the run-time environment.";
        }

        /// <summary>
        /// Provides custom parsing
        /// </summary>
        /// <param name="Src"></param>
        /// <param name="CmdLineArgs"></param>
        /// <returns></returns>

        public static new bool Parse(SettingsSource Src, string[] CmdLineArgs = null)
        {
            if (AppSettingsBase.Parse(Src, CmdLineArgs))
            {
                string s = Settings.Value;
                if (s != null && s != "file" && s != "reg")
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Overrides the basic StringListSetting class to provide special functionality to build a list of DUNS numbers from the command line
        /// </summary>

        private class DUNSListSetting : StringListSetting
        {
            /// <summary>
            /// Initializes the instance. Simply passes control to the parent class constructor
            /// </summary>
            /// <param name="Key"></param>
            /// <param name="ArgValHint"></param>
            /// <param name="DefaultValue"></param>
            /// <param name="Help"></param>

            public DUNSListSetting(string Key, string ArgValHint, List<string> DefaultValue, ArgTyp ArgType, bool Persist, bool IsInternal, string Help)
                : base(Key, ArgValHint, DefaultValue, ArgType, Persist, IsInternal, Help) { }

            /// <summary>
            /// Accepts a value that is either a comma-separated list of DUNS numbers or in the form @filename in which
            /// filename is a file containing DUNS numbers
            /// </summary>
            /// <param name="Key"></param>
            /// <param name="Value"></param>
            /// <returns></returns>

            public override bool Accept(string Key, string Value)
            {
                if (Key.ToLower() == SettingKey.ToLower())
                {
                    if (Value.Substring(0, 1) == "@")
                    {
                        using (StreamReader sr = new StreamReader(Value.Substring(1)))
                        {
                            while ((Value = sr.ReadLine()) != null)
                            {
                                SettingValue.AddRange(Value.Split(','));
                                SettingInitialized = true;
                            }
                        }
                    }
                    else
                    {
                        SettingValue.AddRange(Value.Split(','));
                        SettingInitialized = true;
                    }
                    return true;
                }
                return false;
            }
        }
    }
}
