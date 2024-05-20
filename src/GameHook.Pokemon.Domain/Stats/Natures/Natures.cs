namespace GameHook.Pokemon.Domain.Stats.Natures;

public record Hardy : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Hardy;
    public StatNames StatIncrease { get; init; } = StatNames.Attack;
    public StatNames StatDecrease { get; init; } = StatNames.Attack;
    public byte PersonalityModulo { get; init; } = 0;
}
public record Lonely : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Lonely;
    public StatNames StatIncrease { get; init; } = StatNames.Attack;
    public StatNames StatDecrease { get; init; } = StatNames.Defense;
    public byte PersonalityModulo { get; init; } = 1;
}
public record Brave : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Brave;
    public StatNames StatIncrease { get; init; } = StatNames.Attack;
    public StatNames StatDecrease { get; init; } = StatNames.Speed;
    public byte PersonalityModulo { get; init; } = 2;
}
public record Adamant : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Adamant;
    public StatNames StatIncrease { get; init; } = StatNames.Attack;
    public StatNames StatDecrease { get; init; } = StatNames.SpecialAttack;
    public byte PersonalityModulo { get; init; } = 3;
}
public record Naughty : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Naughty;
    public StatNames StatIncrease { get; init; } = StatNames.Attack;
    public StatNames StatDecrease { get; init; } = StatNames.SpecialDefense;
    public byte PersonalityModulo { get; init; } = 4;
}
public record Bold : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Bold;
    public StatNames StatIncrease { get; init; } = StatNames.Defense;
    public StatNames StatDecrease { get; init; } = StatNames.Attack;
    public byte PersonalityModulo { get; init; } = 5;
}
public record Docile : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Docile;
    public StatNames StatIncrease { get; init; } = StatNames.Defense;
    public StatNames StatDecrease { get; init; } = StatNames.Defense;
    public byte PersonalityModulo { get; init; } = 6;
}
public record Relaxed : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Relaxed;
    public StatNames StatIncrease { get; init; } = StatNames.Defense;
    public StatNames StatDecrease { get; init; } = StatNames.Speed;
    public byte PersonalityModulo { get; init; } = 7;
}
public record Impish : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Impish;
    public StatNames StatIncrease { get; init; } = StatNames.Defense;
    public StatNames StatDecrease { get; init; } = StatNames.SpecialAttack;
    public byte PersonalityModulo { get; init; } = 8;
}
public record Lax : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Lax;
    public StatNames StatIncrease { get; init; } = StatNames.Defense;
    public StatNames StatDecrease { get; init; } = StatNames.SpecialDefense;
    public byte PersonalityModulo { get; init; } = 9;
}
public record Timid : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Timid;
    public StatNames StatIncrease { get; init; } = StatNames.Speed;
    public StatNames StatDecrease { get; init; } = StatNames.Attack;
    public byte PersonalityModulo { get; init; } = 10;
}
public record Hasty : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Hasty;
    public StatNames StatIncrease { get; init; } = StatNames.Speed;
    public StatNames StatDecrease { get; init; } = StatNames.Defense;
    public byte PersonalityModulo { get; init; } = 11;
}
public record Serious : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Serious;
    public StatNames StatIncrease { get; init; } = StatNames.Speed;
    public StatNames StatDecrease { get; init; } = StatNames.Speed;
    public byte PersonalityModulo { get; init; } = 12;
}
public record Jolly : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Jolly;
    public StatNames StatIncrease { get; init; } = StatNames.Speed;
    public StatNames StatDecrease { get; init; } = StatNames.SpecialAttack;
    public byte PersonalityModulo { get; init; } = 13;
}
public record Naive : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Naive;
    public StatNames StatIncrease { get; init; } = StatNames.Speed;
    public StatNames StatDecrease { get; init; } = StatNames.SpecialDefense;
    public byte PersonalityModulo { get; init; } = 14;
}
public record Modest : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Modest;
    public StatNames StatIncrease { get; init; } = StatNames.SpecialAttack;
    public StatNames StatDecrease { get; init; } = StatNames.Attack;
    public byte PersonalityModulo { get; init; } = 15;
}
public record Mild : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Mild;
    public StatNames StatIncrease { get; init; } = StatNames.SpecialAttack;
    public StatNames StatDecrease { get; init; } = StatNames.Defense;
    public byte PersonalityModulo { get; init; } = 16;
}
public record Quiet : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Quiet;
    public StatNames StatIncrease { get; init; } = StatNames.SpecialAttack;
    public StatNames StatDecrease { get; init; } = StatNames.Speed;
    public byte PersonalityModulo { get; init; } = 17;
}
public record Bashful : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Bashful;
    public StatNames StatIncrease { get; init; } = StatNames.SpecialAttack;
    public StatNames StatDecrease { get; init; } = StatNames.SpecialAttack;
    public byte PersonalityModulo { get; init; } = 18;
}
public record Rash : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Rash;
    public StatNames StatIncrease { get; init; } = StatNames.SpecialAttack;
    public StatNames StatDecrease { get; init; } = StatNames.SpecialDefense;
    public byte PersonalityModulo { get; init; } = 19;
}
public record Calm : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Calm;
    public StatNames StatIncrease { get; init; } = StatNames.SpecialDefense;
    public StatNames StatDecrease { get; init; } = StatNames.Attack;
    public byte PersonalityModulo { get; init; } = 20;
}
public record Gentle : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Gentle;
    public StatNames StatIncrease { get; init; } = StatNames.SpecialDefense;
    public StatNames StatDecrease { get; init; } = StatNames.Defense;
    public byte PersonalityModulo { get; init; } = 21;
}
public record Sassy : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Sassy;
    public StatNames StatIncrease { get; init; } = StatNames.SpecialDefense;
    public StatNames StatDecrease { get; init; } = StatNames.Speed;
    public byte PersonalityModulo { get; init; } = 22;
}
public record Careful : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Careful;
    public StatNames StatIncrease { get; init; } = StatNames.SpecialDefense;
    public StatNames StatDecrease { get; init; } = StatNames.SpecialAttack;
    public byte PersonalityModulo { get; init; } = 23;
}
public record Quirky : INature
{
    public NatureNames Nature { get; init; } = NatureNames.Quirky;
    public StatNames StatIncrease { get; init; } = StatNames.SpecialDefense;
    public StatNames StatDecrease { get; init; } = StatNames.SpecialDefense;
    public byte PersonalityModulo { get; init; } = 24;
}