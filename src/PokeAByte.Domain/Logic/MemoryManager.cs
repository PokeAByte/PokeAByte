using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Logic;


/// <summary>    
/// Default implementation of the <see cref="IMemoryManager"/> interface.
/// </summary>
public class MemoryManager : IMemoryManager
{
    public MemoryManager(MemoryAddressBlock[] blocksToRead)
    {
        DefaultNamespace = new StaticMemoryContainer(blocksToRead);
        // DefaultNamespace = new MemoryNamespace();
        Namespaces = new Dictionary<string, IMemoryNamespace>()
        {
            { "default", DefaultNamespace }
        };
    }

    public Memory<byte> GetDefaultMemory(uint firstAdress, uint lastAdress)
    {
        return ((StaticMemoryContainer)DefaultNamespace).GetMemory(firstAdress, lastAdress);
    }

    /// <inheritdoc />
    public Dictionary<string, IMemoryNamespace> Namespaces { get; private set; }

    /// <inheritdoc />
    public IMemoryNamespace DefaultNamespace { get; private set; }

    /// <inheritdoc />
    public IByteArray Get(string? area, MemoryAddress memoryAddress, int length)
    {
        if (area == null || area == "default")
        {
            return DefaultNamespace.get_bytes(memoryAddress, length);
        }
        return Namespaces[area].get_bytes(memoryAddress, length);
    }

    /// <inheritdoc />
    public void Fill(string area, MemoryAddress memoryAddress, byte[] data)
    {
        Namespaces.TryGetValue(area, out IMemoryNamespace? namespaceArea);
        if (namespaceArea == null)
        {
            namespaceArea = new DynamicMemoryContainer();
            Namespaces[area] = namespaceArea;
        }
        namespaceArea.Fill(memoryAddress, data);
    }

    /// <inheritdoc />
    public ReadOnlySpan<byte> GetReadonlyBytes(string? area, uint memoryAddress, int length, bool skipCheck = false)
    {
        if (area == null || area == "default")
        {
            return DefaultNamespace.GetReadonlyBytes(memoryAddress, length, skipCheck);
        }
        return Namespaces[area].GetReadonlyBytes(memoryAddress, length, skipCheck);
    }

    public byte[] GetAllBytes(string? area)
    {
        if (area == null || area == "default")
        {
            return DefaultNamespace.GetAllBytes();
        }
        return Namespaces[area].GetAllBytes();
    }
}
