using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Logic;

public class StaticMemoryContainer : IMemoryNamespace
{
    internal Memory<byte> Data { get; init; }

    internal StaticMemoryContainer(uint size)
    {
        Data = new byte[((int)size) + 1];
    }

    public IList<IByteArray> Fragments => [new ByteArray(0, this.Data.ToArray())];

    public ReadOnlySpan<byte> GetReadonlyBytes(uint address, int length)
    {
        try
        {
            return Data.Span.Slice((int)address, length);
        } catch (ArgumentOutOfRangeException)
        {
            throw new Exception($"Cannot retrieve bytes starting at {address:X2} because getting {length} bytes would overflow the memory container.");
        }
    }

    public byte get_byte(uint address)
    {
        return Data.Span[(int)address];
    }

    public bool Contains(uint address) => address >= 0 && address < Data.Length;

    public IByteArray get_bytes(uint address, int length)
    {
        return new ByteArray(address, GetReadonlyBytes(address, length).ToArray());
    }

    public void Fill(uint address, byte[] data)
    {
        // _accessedRegions.Add((address, data.Length));
        data.AsSpan().CopyTo(Data.Span.Slice((int)address, data.Length));
    }
}