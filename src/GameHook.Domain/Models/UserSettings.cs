namespace GameHook.Domain.Models;

public record UserSettings
{
    public string DefaultDriver { get; init; } = DriverModels.Bizhawk;
    public int DriverTestTimeoutMs { get; init; } = 100;
}