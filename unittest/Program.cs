using System;
using AppSettings;
using System.Collections.Generic;

namespace UnitTest
{
    class Program
    {
        static Cfg cfg = new Cfg();

        static void Main(string[] args)
        {
            try
            {
                args = new string[] { "-duns", "1,2,3", "-batchsize", "123", "-foo", "-frobazz", "wow", "-adatesetting", "2017-12-31"};

                if (!Cfg.Parse(SettingsSource.CommandLine, args))
                {
                    Cfg.ShowUsage();
                    return;
                }

                if (Cfg.Settings.Value == "file")
                {
                    if (!Cfg.Parse(SettingsSource.SettingsFile, args))
                    {
                        Cfg.ShowUsage();
                        return;
                    }
                }
                if (Cfg.Settings.Value == "reg")
                {
                    if (!Cfg.Parse(SettingsSource.Registry, args))
                    {
                        Cfg.ShowUsage();
                        return;
                    }
                }
                Console.WriteLine("----------------------------");
                Cfg.DisplayToConsole();
                Cfg.Hidden.Value = true;
                Console.WriteLine("----------------------------");
                Cfg.DisplayToConsole();

                // test implicit conversion operator
                List<string> DunsTest = Cfg.DUNS;
                int BatchSize = Cfg.BatchSize;
                DateTime ADateSetting = Cfg.ADateSetting;
                string Frobazz = Cfg.Frobazz;

                //cfg.Settings.Value = null;
                //cfg.DUNS.Value = new List<string>("111111111,222222222,333333333".Split(','));
                //cfg.BatchSize.Value = 98765;
                //cfg.InitSettings.Value = false;
                //cfg.Foo.Value = true;
                //cfg.Frobazz.Value = "this is a test\nthis is a test\nthis is a test\nthis is a test\n";
                //cfg.ADateSetting.Value = DateTime.Parse("2016-12-13 05:45:23.195");

                //cfg.Save(SettingsSource.Registry);
                //cfg.Save(SettingsSource.SettingsFile);
            }
            catch (Exception ex)
            {
                if (ex is ParseException)
                {
                    Console.WriteLine(string.Format(ex.Message));
                }
                else
                {
                    Console.WriteLine(string.Format("An exception occurred. The exception was: {0}", ex.Message));
                }
            }
        }
    }
}
