using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Logic;

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
        data.AsSpan().CopyTo(Data.AsSpan(offset, data.Length));
    }

    public bool Contains(MemoryAddress memoryAddress)
    {
        return memoryAddress - StartingAddress < Data.Length;
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
