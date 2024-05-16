namespace GameHook.Contracts.PokemonEmerald;

public class GrowthSubstructure : Substructure
{
    public ushort Species { get; set; }
    public ushort ItemHeld { get; set; }
    public uint Experience { get; set; }
    public byte PpBonuses { get; set; }
    public byte Friendship { get; set; }
    public ushort Unused { get; set; }

    public static GrowthSubstructure FromByteArray(byte[] byteData)
    {
        if (byteData.Length != 12)
            throw new InvalidOperationException(
                "Growth substructure data is not 12 bytes long");
        return new GrowthSubstructure
        {
            Species = BitConverter.ToUInt16(byteData.AsSpan()[..2]),
            ItemHeld = BitConverter.ToUInt16(byteData.AsSpan()[2..4]),
            Experience = BitConverter.ToUInt32(byteData.AsSpan()[4..8]),
            PpBonuses = byteData[8],
            Friendship = byteData[9],
            Unused = BitConverter.ToUInt16(byteData.AsSpan()[10..12])
        };
    }

    public override byte[] AsByteArray()
    {
        var byteData = new List<byte>(12);
        byteData.AddRange(BitConverter.GetBytes(Species));
        byteData.AddRange(BitConverter.GetBytes(ItemHeld));
        byteData.AddRange(BitConverter.GetBytes(Experience));
        byteData.Add(PpBonuses);
        byteData.Add(Friendship);
        byteData.AddRange(BitConverter.GetBytes(Unused));
        return byteData.ToArray();
    }

    public override ushort[] AsUShortArray()
    {
        var xpHigh = (ushort)(Experience >> 16);
        var xpLow = (ushort)(Experience & 0xffff);
        var byteData = new List<ushort>
        {
            Species,
            ItemHeld,
            xpHigh,
            xpLow,
            PpBonuses,
            Friendship,
            Unused
        };
        return byteData.ToArray();
    }
}