using System.Reflection;

namespace GameHook.Domain
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

        public static string BinaryDirectoryGameHookFilePath => Path.Combine(BinaryDirectory, "GameHook.json");

        public static string ConfigurationDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GameHook");

        public static string LogFilePath =>
            Path.Combine(ConfigurationDirectory, "GameHook.log");

        public static string ConfigurationDirectoryAppsettingsFilePath =>
            Path.Combine(ConfigurationDirectory, "appsettings.json");

        public static string ConfigurationDirectoryWpfConfigFilePath =>
            Path.Combine(ConfigurationDirectory, "gamehook.wpf.config");

#if DEBUG
        public static bool IsDebug => true;
        public static bool IsTestingBuild => true;
#else
    public static bool IsDebug = false;
    public static bool IsTestingBuild => AssemblyVersion == "0.0.0.0";
#endif
    }
}