using System.Reflection;

namespace PokeAByte.Domain.Models
{
    public static class BuildEnvironment
    {
        // 0.0.0.0
        public static string AssemblyVersion
        {
            get
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                var attributes = entryAssembly?.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);

                if (attributes != null && attributes.Length > 0)
                {
                    return ((AssemblyFileVersionAttribute)attributes[0]).Version;
                }

                throw new Exception("Cannot determine application AssemblyVersion.");
            }
        }

        private static string BinaryDirectory =>
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
            throw new Exception("Could not determine the binary directory.");

        public static string BinaryDirectoryPokeAByteFilePath => Path.Combine(BinaryDirectory, "PokeAByte.json");

        public static string ConfigurationDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PokeAByte");

        public static string LogFilePath =>
            Path.Combine(ConfigurationDirectory, "PokeAByte.log");
        public static string ConfigurationDirectoryAppsettingsFilePath =>
            Path.Combine(ConfigurationDirectory, "appsettings.json");
        public static string UserSettingsJson =>
            Path.Combine(ConfigurationDirectory, "user_settings.json");
        public static string MapperSettingsJson =>
            Path.Combine(ConfigurationDirectory, "saved_mapper_settings.json");

#if DEBUG
        public static bool IsDebug => true;
        public static bool IsTestingBuild => true;

#else
    public static bool IsDebug = false;
    public static bool IsTestingBuild => AssemblyVersion == "0.0.0.0";
#endif
    }
}