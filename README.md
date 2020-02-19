# C# Console Application command line parser

A C# DLL project for console applications to parse command-line args, and handle persistent settings in the Windows registry and/or a settings file. Parses command line args (or program settings from a file or the registry), displays formatted usage instructions, supports customized parsing and validation.

# Usage

Create a console application, and add a reference to the `appsettings` DLL. Then, create a subclass of the `AppSettingsBase` class in your project:

```
using AppSettings;

namespace MyConsoleApp
{
    class Cfg : AppSettingsBase
    {
    }
}
```
Next, define a static constructor that initializes the args that you want to support on the command line. There are several types of args: boolean, string, date, int, etc. This example defines an int, a string, and a bool setting for a hypothetical command line utility:

```
namespace MyConsoleApp
{
    class Cfg : AppSettingsBase
    {
        static Cfg()
        {
            SettingList = new Setting[] {
                new IntSetting("BatchSize", "n", 100, Setting.ArgTyp.Optional, true, false,
                    "Specifies n as the batch size. This supports some hypothetical batch size. " +
                    "If not provided, 100 is used as the default value."),
                new StringSetting("Foo", "zzz", null,  Setting.ArgTyp.Optional, false, false,
                    "Provide some free-form text value."),
                new BoolSetting("Init", false,  Setting.ArgTyp.Optional, false, false,
                    "Performs some initialization. If not specified on the command line, then no " +
                    "special initialization is performed."),
            };
            Usage =
                "This variable allows you to specify a usage synopsis. The synopsis is displayed " +
                "along with the help";
        }
    }
}
```
Add variables to hold the parsed values. Notice that the getter returns the value from the base class settings dictionary using the same key - "Batchsize" in the first case - that was used above in the matching constructor.
```
namespace MyConsoleApp
{
    class Cfg : AppSettingsBase
    {
        static Cfg()
        {
            ...
        }
        public static IntSetting BatchSize { get { return (IntSetting)SettingsDict["BatchSize"]; } }
        public static StringSetting Foo { get { return (StringSetting)SettingsDict["Foo"]; } }
        public static BoolSetting Init { get { return (BoolSetting)SettingsDict["Init"]; } }
    }
}
```
If you have special parsing above and beyond the built-in parsing, override the `Parse` Method. In your override, first call the base class parse and if that fails, implement your custom parsing. This example requires that the `Foo` field be either 'bar' or 'baz':
```
namespace MyConsoleApp
{
    class Cfg : AppSettingsBase
    {
        ...
        public static new bool Parse(SettingsSource Src, string[] CmdLineArgs = null)
        {
            if (AppSettingsBase.Parse(Src, CmdLineArgs))
            {
                string s = Foo.Value;
                if (s != null && s != "bar" && s != "baz")
                {
                    ParseErrorMessage = "Invalid value for the Foo arg. Valid values are 'bar' and 'baz'";
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
```
Now you've defined how you want the command line to be parsed. Now reference the args in your Main class:
```
using AppSettings;
using System;

namespace MyConsoleApp
{
    class Program
    {
        static Cfg cfg = new Cfg();

        static void Main(string[] args)
        {
            if (!Cfg.Parse(SettingsSource.CommandLine, args))
            {
                if (AppSettingsBase.ParseErrorMessage != null)
                {
                    Console.WriteLine(AppSettingsBase.ParseErrorMessage);
                }
                else
                {
                    AppSettingsBase.ShowUsage();
                }
                return;
            }
            Console.WriteLine(string.Format("You entered {0} on for the BatchSize arg", (int)Cfg.BatchSize));
        }
    }
}
```
All of the args defined in this example were optional. Normally in that case running the console application with no args would result in a successful parse. However because the `Foo` arg was defined with a default of null, the parser assigned it the empty string and so the custom validation failed. If any mandatory arg was not specified then the parse would also fail and your Main method would display the usage. (You could also define a -help arg explicitly to support displaying help.)

To run the program, you would specify something like this on the command line:
```
myconsoleapp -batchsize 42 -foo bar -init
```
In the above example,  `42` is parsed to the `BatchSize` variable, `bar` is parsed to the `Foo` variable, and `True` is parsed to the `Init` variable.
