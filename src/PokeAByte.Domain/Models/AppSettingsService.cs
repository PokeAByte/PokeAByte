using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;

namespace PokeAByte.Domain;

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
        _settings = JsonFile.Read(_settingsPath, DomainJson.Default.AppSettings) ?? new();
    }

    public void Save()
    {
        JsonFile.Write(this._settings, _settingsPath, DomainJson.Default.AppSettings);
    }
}