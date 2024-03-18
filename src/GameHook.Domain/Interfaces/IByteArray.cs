namespace GameHook.Domain.Interfaces
{
    public interface IByteArray
    {
        MemoryAddress StartingAddress { get; }

        byte[] Data { get; }

        void Fill(int offset, byte[] data);

        bool Contains(MemoryAddress addr);

        IByteArray Slice(int offset, int length);

        IByteArray[] Chunk(int size);

        byte get_byte(int offset = 0);
        public ushort get_uint16_le(int offset = 0) => (ushort)((get_byte(offset + 0) << 0) | (get_byte(offset + 1) << 8));
        public ushort get_uint16_be(int offset = 0) => (ushort)((get_byte(offset + 0) << 8) | (get_byte(offset + 1) << 0));
        public uint get_uint32_le(int offset = 0) => (uint)((get_uint16_le(offset + 0) << 0) | (get_uint16_le(offset + 2) << 16));
        public uint get_uint32_be(int offset = 0) => (uint)((get_uint16_be(offset + 0) << 16) | (get_uint16_be(offset + 2) << 0));
        public ulong get_uint64_le(int offset = 0) => (ulong)((get_uint32_le(offset + 0) << 0) | (get_uint32_le(offset + 4) << 32));
        public ulong get_uint64_be(int offset = 0) => (ulong)((get_uint32_be(offset + 0) << 32) | (get_uint32_be(offset + 4) << 0));
    }
}