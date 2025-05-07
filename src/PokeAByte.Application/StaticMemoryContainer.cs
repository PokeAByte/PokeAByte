using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Application;

public class StaticMemoryContainer : IMemoryNamespace
{
    internal Memory<byte> Data { get; init; }

    internal StaticMemoryContainer(uint size)
    {
        Data = new byte[((int)size)+1];
    }

    public IList<IByteArray> Fragments => [new ByteArray(0, this.Data.ToArray())];

    public ReadOnlySpan<byte> GetReadonlyBytes(uint address, int length)
    {
        return Data.Span.Slice((int)address, length);
    }

    public byte get_byte(uint address)
    {
        return Data.Span[(int)address];
    }

    public bool Contains(uint address) => address >= 0 && address < Data.Length;

    public IByteArray get_bytes(uint address, int length)
    {
        return new ByteArray(address, Data.Slice((int)address, length).ToArray());
    }

    public void Fill(uint address, byte[] data)
    {
        data.AsSpan().CopyTo(Data.Span.Slice((int)address, data.Length));
    }
}