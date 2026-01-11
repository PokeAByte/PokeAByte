
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;

public class AppSettingsService
{
    private static string _settingsPath = Path.Combine(BuildEnvironment.ConfigurationDirectory, "settings.json");
    private AppSettings _settings;

    public AppSettingsService(ILogger<AppSettingsService> logger)
    {
        this.Load();
        logger.LogInformation($"AppSettings initialized: DELAY_MS_BETWEEN_READS: {_settings.DELAY_MS_BETWEEN_READS}.");
    }

    public AppSettings Get()
    {
        return _settings;
    }

    public void Set(AppSettings value)
    {
        _settings = value;
    }

    [MemberNotNull(nameof(_settings))]
    private void Load()
    {
        _settings = JsonFile.Deserialize(_settingsPath, MapperDtoContext.Default.AppSettings, new());
    }

    public void Save()
    {
        var settingsText = JsonSerializer.Serialize(
            this._settings,
            new JsonSerializerOptions()
            {
                WriteIndented = true,
                TypeInfoResolver = MapperDtoContext.Default
            }
        );
        File.WriteAllText(
            _settingsPath,
            settingsText
        );
    }
}