namespace GameHook.Contracts.PokemonEmerald;

public abstract class Substructure
{
    public abstract byte[] AsByteArray();
    public abstract ushort[] AsUShortArray();
    
    public ushort GetSum()
    {
        uint val = 0;
        var data = AsByteArray();
        for (int i = 0; i < data.Length; i+=4)
            val += BitConverter.ToUInt32(data, i);
        return (ushort)(val + (val >> 16));
    }
}