using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Logic;

public class DynamicMemoryContainer : IMemoryNamespace
{
    public IList<IByteArray> Fragments { get; } = new List<IByteArray>();
    public bool IsDirty { get; private set; } = false;

    public void ClearDirtyFlag() => IsDirty = false;
    public void SetDirtyFlag() => IsDirty = true;

    public void Fill(MemoryAddress memoryAddress, byte[] data)
    {
        int filledFragments = 0;

        for (int i = 0; i < Fragments.Count; i++)
        {
            IByteArray fragment = Fragments[i];
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

    public ReadOnlySpan<byte> GetReadonlyBytes(MemoryAddress memoryAddress, int length, bool skipCheck = false)
    {
        if (length < 0)
        {
            throw new Exception($"Cannot retrieve bytes starting at {memoryAddress.ToHexdecimalString()} because getting {length} bytes is invalid.");
        }
        for (int i = 0; i < Fragments.Count; i++)
        {
            IByteArray fragment = Fragments[i];
            if (fragment.Contains(memoryAddress))
            {
                int offset = (int)(memoryAddress - fragment.StartingAddress);

                if (offset < 0 || (offset + length) > fragment.Data.Length)
                {
                    throw new Exception($"Cannot retrieve bytes starting at {memoryAddress.ToHexdecimalString()} (starting address at {fragment.StartingAddress.ToHexdecimalString()} because getting {length} bytes would overflow the fragment array.");
                }
                return fragment.Data.AsSpan(offset, length);
            }
        }

        throw new Exception($"Memory address {memoryAddress.ToHexdecimalString()} is not contained in any fragment in the namespace.");
    }

    public byte get_byte(MemoryAddress memoryAddress) => get_bytes(memoryAddress, 1).get_byte();

    public byte[] GetAllBytes()
    {
        // TODO: Container might have multiple fragments.
        return this.Fragments.FirstOrDefault()?.Data ?? [];
    }

    public byte[] get_raw_bytes(uint memoryAddress, int length)
    {
        return GetReadonlyBytes(memoryAddress, length).ToArray();
    }
}
