namespace GameHook.Pokemon.Domain.Stats.Natures;

public interface INature
{
    public NatureNames Nature { get; init; }
    public StatNames StatIncrease { get; init; }
    public StatNames StatDecrease { get; init; }
    public byte PersonalityModulo { get; init; }
}