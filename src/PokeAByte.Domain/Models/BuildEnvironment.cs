using System.Reflection;

namespace PokeAByte.Domain.Models;

public static class BuildEnvironment
{
    private static string BinaryDirectory =>
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
        throw new Exception("Could not determine the binary directory.");

    public static string BinaryDirectoryPokeAByteFilePath => Path.Combine(BinaryDirectory, "PokeAByte.json");

    public static string ConfigurationDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PokeAByte");

    public static string LogFilePath => Path.Combine(ConfigurationDirectory, "PokeAByte.log");

#if DEBUG
    public static bool IsDebug => true;
#else
    public static bool IsDebug = false;
#endif
}