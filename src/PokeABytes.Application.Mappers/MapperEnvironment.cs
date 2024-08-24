﻿using PokeAByte.Domain.Models;

namespace PokeAByte.Application.Mappers;

public static class MapperEnvironment
{
    public static string MapperUpdateSettingsFile =>
        Path.Combine(BuildEnvironment.ConfigurationDirectory, "mapper_updater_settings.json");
    public static string GithubApiSettings =>
        Path.Combine(BuildEnvironment.ConfigurationDirectory, "github_api_settings.json");
    public static string MapperTreeJson => "mapper_tree.json";
    public static string OutdatedMapperTreeJson => 
        Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers/outdated_mapper_tree.json");
    public static string GithubChangeFilePath =>
        Path.Combine(BuildEnvironment.ConfigurationDirectory, MapperTreeJson);
    public static string MapperLocalDirectory => 
        Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers");
    public static string MapperLocalArchiveDirectory => 
        Path.Combine(BuildEnvironment.ConfigurationDirectory, "MapperArchives");
    public static string MapperArchiveSavedDataJson => 
        Path.Combine(BuildEnvironment.ConfigurationDirectory, "mapper_archive_saved_data.json");
    public static string MapperLocalCommitHashFilePath => 
        Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers", "COMMIT_HASH.txt");
    public static string MapperTemporaryZipFilepath => 
        Path.Combine(BuildEnvironment.ConfigurationDirectory, $"mappers_tmp.zip");
    public static string MapperTemporaryExtractionDirectory => 
        Path.Combine(BuildEnvironment.ConfigurationDirectory, $"mappers_tmp\\");
    public static string MapperArchiveDirectory => 
        Path.Combine(MapperLocalDirectory, "Archive");
}