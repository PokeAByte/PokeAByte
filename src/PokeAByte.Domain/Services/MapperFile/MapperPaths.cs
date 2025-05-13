using PokeAByte.Domain.Models;

namespace PokeAByte.Domain.Services.MapperFile;

public static class MapperPaths
{
    public static string MapperUpdateSettingsFile { get; }
    public static string GithubApiSettings { get; }
    public static string MapperTreeJson { get; }
    public static string OutdatedMapperTreeJson { get; }
    public static string MapperDirectory { get; }
    public static string? LocalMapperDirectory {
        get {
            var processPath = Path.GetDirectoryName(Environment.ProcessPath);           
            return processPath != null 
                ? Path.Combine(processPath, "mappers")
                : null;
        }
    }

    public static string MapperLocalArchiveDirectory { get; }
    public static string MapperArchiveDirectory { get; }

    static MapperPaths()
    {
        MapperUpdateSettingsFile = Path.Combine(BuildEnvironment.ConfigurationDirectory, "mapper_updater_settings.json");
        GithubApiSettings = Path.Combine(BuildEnvironment.ConfigurationDirectory, "github_api_settings.json");
        MapperTreeJson = "mapper_tree.json";
        OutdatedMapperTreeJson = Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers/outdated_mapper_tree.json");
        MapperDirectory = Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers");
        
        MapperLocalArchiveDirectory = Path.Combine(BuildEnvironment.ConfigurationDirectory, "MapperArchives");
        MapperArchiveDirectory = Path.Combine(MapperDirectory, "Archive");
    }
}
