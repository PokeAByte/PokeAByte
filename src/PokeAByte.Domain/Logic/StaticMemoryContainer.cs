using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Logic;

public class StaticMemoryContainer : IMemoryNamespace
{
    private MemoryAddressBlock[] _blocks;

    internal Memory<byte> Data { get; init; }

    internal StaticMemoryContainer(MemoryAddressBlock[] blocksToRead)
    {
        this._blocks = blocksToRead;
        var lastBlock = blocksToRead.OrderByDescending(x => x.EndingAddress).First();
        var size = lastBlock.EndingAddress;
        Data = new byte[((int)size) + 1];
    }

    public IList<IByteArray> Fragments => [new ByteArray(0, this.Data.ToArray())];

    private bool CheckRange(uint start, int length)
    {
        var end = start + length - 1;
        foreach (var block in _blocks)
        {
            if (start >= block.StartingAddress && end <= block.EndingAddress)
            {
                return true;
            }
        }
        return false;
    }

    public ReadOnlySpan<byte> GetReadonlyBytes(uint address, int length)
    {
        if (!CheckRange(address, length))
        {
            throw new Exception(
                $"Invalid memory read: {address:X2} - {address + length:X2} is outside of mapped memory regions."
            );
        }
        try
        {
            return Data.Span.Slice((int)address, length);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new Exception($"Cannot retrieve bytes starting at {address:X2} because getting {length} bytes would overflow the memory container.");
        }
    }

    public byte get_byte(uint address)
    {
        return GetReadonlyBytes(address, 1)[0];
    }

    public bool Contains(uint address) => address >= 0 && address < Data.Length;

    public IByteArray get_bytes(uint address, int length)
    {
        return new ByteArray(address, GetReadonlyBytes(address, length).ToArray());
    }

    public void Fill(uint address, byte[] data)
    {
        data.AsSpan().CopyTo(Data.Span.Slice((int)address, data.Length));
    }
}