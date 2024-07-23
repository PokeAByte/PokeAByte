namespace PokeAByte.Web.Models;

public record PropertySettingsModel
{
    public required string PropertyName { get; init; }
    public required string PropertyPath { get; init; }
    public bool IsExpanded { get; set; }
    public bool HasChildren { get; init; }
    public bool HasProperty { get; init; }
}