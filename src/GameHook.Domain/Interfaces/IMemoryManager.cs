namespace GameHook.Domain.Interfaces
{
    public interface IMemoryManager
    {
        Dictionary<string, IMemoryNamespace> Namespaces { get; }

        IMemoryNamespace DefaultNamespace { get; }

        IByteArray Get(string? area, MemoryAddress memoryAddress, int length);
        void Fill(string area, MemoryAddress memoryAddress, byte[] data);
    }

    public interface IMemoryNamespace
    {
        ICollection<IByteArray> Fragments { get; }

        public void Fill(MemoryAddress memoryAddress, byte[] data);
        bool Contains(MemoryAddress memoryAddress);

        byte get_byte(MemoryAddress memoryAddress);
        IByteArray get_bytes(MemoryAddress memoryAddress, int length);

        public ushort get_uint16_le(MemoryAddress memoryAddress) => (ushort)((get_byte(memoryAddress + 0) << 0) | (get_byte(memoryAddress + 1) << 8));
        public ushort get_uint16_be(MemoryAddress memoryAddress) => (ushort)((get_byte(memoryAddress + 0) << 8) | (get_byte(memoryAddress + 1) << 0));
        public uint get_uint32_le(MemoryAddress memoryAddress) => (uint)((get_uint16_le(memoryAddress + 0) << 0) | (get_uint16_le(memoryAddress + 2) << 16));
        public uint get_uint32_be(MemoryAddress memoryAddress) => (uint)((get_uint16_be(memoryAddress + 0) << 16) | (get_uint16_be(memoryAddress + 2) << 0));
        public ulong get_uint64_le(MemoryAddress memoryAddress) => (ulong)((get_uint32_le(memoryAddress + 0) << 0) | (get_uint32_le(memoryAddress + 4) << 32));
        public ulong get_uint64_be(MemoryAddress memoryAddress) => (ulong)((get_uint32_be(memoryAddress + 0) << 32) | (get_uint32_be(memoryAddress + 4) << 0));
    }
}
