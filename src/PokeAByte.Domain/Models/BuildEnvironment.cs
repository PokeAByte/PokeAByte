namespace PokeAByte.Domain.Models;

public static class BuildEnvironment
{
    public static string ConfigurationDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PokeAByte"
    );

    public static string LogFilePath => Path.Combine(ConfigurationDirectory, "PokeAByte.log");
}