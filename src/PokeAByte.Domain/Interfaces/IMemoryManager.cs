namespace PokeAByte.Domain.Interfaces
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

        public ushort get_uint16_le(MemoryAddress memoryAddress)
        {
            var bytes = get_bytes(memoryAddress, 2).Data;
            return (ushort)((bytes[0] << 0) | (bytes[1] << 8));
        }

        public ushort get_uint16_be(MemoryAddress memoryAddress)
        {
            var bytes = get_bytes(memoryAddress, 2).Data;
            return (ushort)((bytes[0] << 8) | (bytes[1] << 0));
        }

        public uint get_uint32_le(MemoryAddress memoryAddress)
        {
            var bytes = get_bytes(memoryAddress, 4).Data;
            int lower = (bytes[0] << 0) | (bytes[1] << 8);
            int upper = (bytes[2] << 0) | (bytes[3] << 8);
            return (uint)((lower << 0) | (upper << 16));
        }

        public uint get_uint32_be(MemoryAddress memoryAddress)
        {
            var bytes = get_bytes(memoryAddress, 4).Data;
            int lower = (bytes[0] << 8) | (bytes[1] << 0);
            int upper = (bytes[2] << 8) | (bytes[3] << 0);
            return (uint)((lower << 16) | (upper << 0));
        }

        public ulong get_uint64_le(MemoryAddress memoryAddress) => (ulong)((get_uint32_le(memoryAddress + 0) << 0) | (get_uint32_le(memoryAddress + 4) << 32));
        public ulong get_uint64_be(MemoryAddress memoryAddress) => (ulong)((get_uint32_be(memoryAddress + 0) << 32) | (get_uint32_be(memoryAddress + 4) << 0));
    }
}
