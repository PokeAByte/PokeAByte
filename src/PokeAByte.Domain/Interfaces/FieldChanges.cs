namespace PokeAByte.Domain.Interfaces;

[Flags]
public enum FieldChanges : short
{
    None = 0,
    Bytes = 1 << 0,
    Value = 1 << 1,
    Address = 1 << 2,
    IsFrozen = 1 << 3,
    Length = 1 << 4,
    Size = 1 << 5,
    Bits = 1 << 6,
    Reference = 1 << 7,
    Description = 1 << 8,
    MemoryContainer = 1 << 9,
}
