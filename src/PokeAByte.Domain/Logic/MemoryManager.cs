using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Logic;


/// <summary>    
/// Default implementation of the <see cref="IMemoryManager"/> interface.
/// </summary>
public class MemoryManager : IMemoryManager
{
    public MemoryManager(uint gameMemorySize)
    {
        DefaultNamespace = new StaticMemoryContainer(gameMemorySize);
        Namespaces = new Dictionary<string, IMemoryNamespace>()
        {
            { "default", DefaultNamespace }
        };
    }

    /// <inheritdoc />
    public Dictionary<string, IMemoryNamespace> Namespaces { get; private set; }

    /// <inheritdoc />
    public IMemoryNamespace DefaultNamespace { get; private set; }

    /// <inheritdoc />
    public IByteArray Get(string? area, MemoryAddress memoryAddress, int length)
    {
        if (area == null || area == "default" )
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
            namespaceArea = new MemoryNamespace();
            Namespaces[area] = namespaceArea;
        }
        namespaceArea.Fill(memoryAddress, data);
    }

    /// <inheritdoc />
    public ReadOnlySpan<byte> GetReadonlyBytes(string? area, uint memoryAddress, int length)
    {
        if (area == null || area == "default")
        {
            return DefaultNamespace.GetReadonlyBytes(memoryAddress, length);
        }
        return Namespaces[area].GetReadonlyBytes(memoryAddress, length);
    }
}

public class MemoryNamespace : IMemoryNamespace
{
    public IList<IByteArray> Fragments { get; } = new List<IByteArray>();

    public void Fill(MemoryAddress memoryAddress, byte[] data)
    {
        int filledFragments = 0;

        for (int i = 0; i < Fragments.Count; i++)
        {
            IByteArray? fragment = Fragments[i];
            if (fragment.Contains(memoryAddress))
            {
                try
                {
                    var offset = (int)(memoryAddress - fragment.StartingAddress);

                    fragment.Fill(offset, data);

                    filledFragments += 1;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Cannot fill {memoryAddress} (starting address of {fragment.StartingAddress}) with {data.Length} bytes of data.", ex);
                }
            }
        }

        if (filledFragments == 0)
        {
            Fragments.Add(new ByteArray(memoryAddress, data));
        }
    }

    public bool Contains(MemoryAddress memoryAddress) => Fragments.Any(fragment => fragment.Contains(memoryAddress));

    public IByteArray get_bytes(MemoryAddress memoryAddress, int length)
    {
        return new ByteArray(memoryAddress, GetReadonlyBytes(memoryAddress, length).ToArray());
    }

    public ReadOnlySpan<byte> GetReadonlyBytes(MemoryAddress memoryAddress, int length)
    {
        for (int i = 0; i < Fragments.Count; i++)
        {
            IByteArray? fragment = Fragments[i];
            if (fragment.Contains(memoryAddress))
            {
                int offset = (int)(memoryAddress - fragment.StartingAddress);

                if (offset < 0 || offset >= fragment.Data.Length || length < 0 || (offset + length) > fragment.Data.Length)
                {
                    throw new Exception($"Cannot retrieve bytes starting at {memoryAddress.ToHexdecimalString()} (starting address at {fragment.StartingAddress.ToHexdecimalString()} because getting {length} bytes would overflow the fragment array.");
                }
                return fragment.Data.AsSpan()[offset..(offset + length)];
            }
        }

        throw new Exception($"Memory address {memoryAddress.ToHexdecimalString()} is not contained in any fragment in the namespace.");
    }

    public byte get_byte(MemoryAddress memoryAddress) => get_bytes(memoryAddress, 1).get_byte(0);
}

public class ByteArray : IByteArray
{
    public ByteArray(MemoryAddress startingAddress, byte[] data)
    {
        StartingAddress = startingAddress;
        Data = data;
    }

    public MemoryAddress StartingAddress { get; }
    public byte[] Data { get; }


    public void Fill(int offset, byte[] data)
    {
        // Check if the offset is negative or beyond the bounds of the destination array
        if (offset < 0 || offset >= Data.Length)
        {
            throw new Exception($"Offset {offset} is out of range of the data array length of {data.Length}.");
        }

        // Check if the destination array has enough space
        if (data.Length > Data.Length - offset)
        {
            throw new Exception($"The destination array is not long enough. The destination array has a length of {Data.Length} where the source array has a length of {data.Length}.");
        }
        data.AsSpan().CopyTo(Data);
    }

    public bool Contains(MemoryAddress memoryAddress)
    {
        var relativeAddr = memoryAddress - StartingAddress;
        return relativeAddr < Data.Length;
    }

    public IByteArray Slice(int offset, int length)
    {
        return new ByteArray(StartingAddress + (uint)offset, Data[offset..(offset + length)]);
    }

    public IByteArray[] Chunk(int size)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size));

        int chunkCount = (Data.Length + size - 1) / size;
        var chunks = new List<IByteArray>();

        for (int i = 0; i < chunkCount; i++)
        {
            int offset = i * size;
            int chunkSize = Math.Min(size, Data.Length - offset);

            chunks.Add(Slice(offset, chunkSize));
        }

        return chunks.ToArray();
    }

    public byte get_byte(int offset)
    {
        return Data[offset];
    }
}
