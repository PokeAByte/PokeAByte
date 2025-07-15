using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Logic;

public class StaticMemoryContainer : IMemoryNamespace
{
    private MemoryAddressBlock[] _blocks;
    private uint _offset;

    internal Memory<byte> Data { get; init; }

    internal StaticMemoryContainer(MemoryAddressBlock[] blocksToRead)
    {
        this._blocks = blocksToRead;
        var lastBlock = blocksToRead.OrderByDescending(x => x.EndingAddress).First();
        var firstBlock = blocksToRead.OrderBy(x => x.StartingAddress).First();
        _offset = firstBlock.StartingAddress;
        var size = lastBlock.EndingAddress - _offset;
        Data = new byte[size + 1];
    }

    internal Memory<byte> GetMemory(uint firstAddress, uint lastAddress)
    {
        int length = (int)(lastAddress - firstAddress);
        if (!this.CheckRange(firstAddress, length)) {
            throw new ArgumentException($"Address range ({firstAddress:X} - {firstAddress+length:X}) not within the memory boundaries.");
        }
        return Data.Slice((int)(firstAddress-_offset), length);
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
                $"Invalid memory read: {address:X2} - {address + length:X2} is outside of available memory regions. Check mapper."
            );
        }
        try
        {
            return Data.Span.Slice((int)(address-_offset), length);
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
        data.AsSpan().CopyTo(Data.Span.Slice((int)(address-_offset), data.Length));
    }
}