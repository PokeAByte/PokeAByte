namespace GameHook.Pokemon.Domain.Stats;

public record BaseStats
{
    public ushort Hp { get; set; }
    public ushort Attack { get; set; }
    public ushort Defense { get; set; }
    public ushort Speed { get; set; }
    public ushort SpecialAttack { get; set; }
    public ushort SpecialDefense { get; set; }
    public ushort Total { get; set; }
}