namespace GameHook.Contracts.PokemonEmerald;

public class MiscellaneousSubstructure : Substructure
{
    public byte PokerusStatus { get; set; }
    public byte MetLocation { get; set; }
    public ushort Origins { get; set; }
    public uint IvsEggAbility { get; set; }
    public uint RibbonsObedience { get; set; }

    public static MiscellaneousSubstructure FromByteArray(byte[] byteData)
    {
        if (byteData.Length != 12)
            throw new InvalidOperationException(
                "Miscellaneous substructure data is not 12 bytes long");
        return new MiscellaneousSubstructure()
        {
            PokerusStatus = byteData[0],
            MetLocation = byteData[1],
            Origins = BitConverter.ToUInt16(byteData.AsSpan()[2..4]),
            IvsEggAbility = BitConverter.ToUInt32(byteData.AsSpan()[4..8]),
            RibbonsObedience = BitConverter.ToUInt32(byteData.AsSpan()[8..12])
        };
    }

    public override byte[] AsByteArray()
    {
        var byteData = new List<byte>(12)
        {
            PokerusStatus,
            MetLocation
        };
        byteData.AddRange(BitConverter.GetBytes(Origins));
        byteData.AddRange(BitConverter.GetBytes(IvsEggAbility));
        byteData.AddRange(BitConverter.GetBytes(RibbonsObedience));
        return byteData.ToArray();
    }

    public override ushort[] AsUShortArray()
    {
        var ivHigh = (ushort)(IvsEggAbility >> 16);
        var ivLow = (ushort)(IvsEggAbility & 0xffff);
        var ribbonsHigh = (ushort)(RibbonsObedience >> 16);
        var ribbonsLow = (ushort)(RibbonsObedience & 0xffff);
        var byteData = new List<ushort>()
        {
            PokerusStatus,
            MetLocation,
            Origins,
            ivHigh,
            ivLow,
            ribbonsHigh,
            ribbonsLow
        };
        return byteData.ToArray();   
    }
}