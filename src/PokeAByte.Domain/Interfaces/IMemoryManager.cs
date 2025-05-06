using System.Buffers.Binary;

namespace PokeAByte.Domain.Interfaces;

public interface IMemoryManager
{
    Dictionary<string, IMemoryNamespace> Namespaces { get; }

    IMemoryNamespace DefaultNamespace { get; }

    IByteArray Get(string? area, MemoryAddress memoryAddress, int length);
    ReadOnlySpan<byte> GetReadonlyBytes(string? area, MemoryAddress memoryAddress, int length);
    void Fill(string area, MemoryAddress memoryAddress, byte[] data);
}

public interface IMemoryNamespace
{
    IList<IByteArray> Fragments { get; }

    public void Fill(MemoryAddress memoryAddress, byte[] data);
    bool Contains(MemoryAddress memoryAddress);
    ReadOnlySpan<byte> GetReadonlyBytes(MemoryAddress memoryAddress, int length);

    byte get_byte(MemoryAddress memoryAddress);

    IByteArray get_bytes(MemoryAddress memoryAddress, int length);

    public ushort get_uint16_le(MemoryAddress memoryAddress)
    {
        var bytes = GetReadonlyBytes(memoryAddress, 2);
        return BinaryPrimitives.ReadUInt16LittleEndian(bytes);
    }

    public ushort get_uint16_be(MemoryAddress memoryAddress)
    {
        var bytes = GetReadonlyBytes(memoryAddress, 2);
        return BinaryPrimitives.ReadUInt16BigEndian(bytes);
    }

    public uint get_uint32_le(MemoryAddress memoryAddress)
    {
        var bytes = GetReadonlyBytes(memoryAddress, 4);
        return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
    }

    public uint get_uint32_be(MemoryAddress memoryAddress)
    {
        var bytes = GetReadonlyBytes(memoryAddress, 4);
        return BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }

    public ulong get_uint64_le(MemoryAddress memoryAddress) => (ulong)((get_uint32_le(memoryAddress + 0) << 0) | (get_uint32_le(memoryAddress + 4) << 32));
    public ulong get_uint64_be(MemoryAddress memoryAddress) => (ulong)((get_uint32_be(memoryAddress + 0) << 32) | (get_uint32_be(memoryAddress + 4) << 0));
}
