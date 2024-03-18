using GameHook.Domain.Interfaces;

namespace GameHook.Domain.Implementations
{
    public class MemoryManager : IMemoryManager
    {
        public MemoryManager()
        {
            Namespaces = new Dictionary<string, IMemoryNamespace>()
            {
                { "default", new MemoryNamespace() }
            };

            DefaultNamespace = Namespaces["default"];
        }

        public Dictionary<string, IMemoryNamespace> Namespaces { get; private set; }
        public IMemoryNamespace DefaultNamespace { get; }

        public IByteArray Get(string? area, MemoryAddress memoryAddress, int length)
        {
            if (area == "default" || area == null) { return DefaultNamespace.get_bytes(memoryAddress, length); }
            else return Namespaces[area].get_bytes(memoryAddress, length);
        }

        public void Fill(string area, MemoryAddress memoryAddress, byte[] data)
        {
            if (Namespaces.ContainsKey(area) == false)
            {
                Namespaces[area] = new MemoryNamespace();
            }

            Namespaces[area].Fill(memoryAddress, data);
        }
    }

    public class MemoryNamespace : IMemoryNamespace
    {
        public ICollection<IByteArray> Fragments { get; } = new List<IByteArray>();

        public void Fill(MemoryAddress memoryAddress, byte[] data)
        {
            int filledFragments = 0;

            foreach (var fragment in Fragments)
            {
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
            foreach (var fragment in Fragments)
            {
                if (fragment.Contains(memoryAddress))
                {
                    var offset = memoryAddress - fragment.StartingAddress;

                    if (offset < 0 || offset >= fragment.Data.Length || length < 0 || (offset + length) > fragment.Data.Length)
                    {
                        throw new Exception($"Cannot retrieve bytes starting at {memoryAddress.ToHexdecimalString()} (starting address at {fragment.StartingAddress.ToHexdecimalString()} because getting {length} bytes would overflow the fragment array.");
                    }

                    return fragment.Slice((int)offset, length);
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

            Array.Copy(data, 0, Data, offset, data.Length);
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
}
