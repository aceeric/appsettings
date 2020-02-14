
namespace AppSettings
{
    /// <summary>
    /// Defines the sources that can can be used to obtain configuration settings from
    /// </summary>

    public enum SettingsSource
    {
        /// <summary>
        /// Indicates that the Windows registry is to be used as the source of application settings.
        /// </summary>
        Registry,
        /// <summary>
        /// Indicates that the a settings file in the executable directory is to be used as the source of application settings.
        /// </summary>
        SettingsFile,
        /// <summary>
        /// Indicates that the command line is to be used as the source of application settings. Note: persistance is not supported for this source
        /// </summary>
        CommandLine
    }
}
