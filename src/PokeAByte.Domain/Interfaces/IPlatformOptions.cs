namespace PokeAByte.Domain.Interfaces
{
    public interface IPlatformOptions
    {
        public EndianTypes EndianType { get; }

        public MemoryAddressBlock[] Ranges { get; }

        uint MemorySize { get; }
    }
}
