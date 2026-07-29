using System.Buffers.Binary;

namespace PokeAByte.Domain.Interfaces;

/// <summary>
/// Container for a section of game memory with some utility functions attached.
/// </summary>
public interface IByteArray
{
    /// <summary>
    /// The starting address, relative to the parent <see cref="IMemoryNamespace"/>, of the section.
    /// </summary>
    /// <remarks> May be used from within a mapper script. </remarks>
    MemoryAddress StartingAddress { get; }

    /// <summary>
    /// The raw memory bytes of the section.
    /// </summary>
    /// <remarks> Can be used from within a mapper script. </remarks>
    byte[] Data { get; }

    /// <summary>
    /// Writes the target bytes, starting at the provided offset, into <see cref="Data"/>.
    /// </summary>
    /// <param name="offset"> The first position to start writing bytes into. </param>
    /// <param name="data"> The bytes to write. </param>
    /// <remarks> May be used from within a mapper script. </remarks>
    void Fill(int offset, byte[] data);

    /// <summary>
    /// Checks if the target adress is within the bounds of the memory section.
    /// </summary>
    /// <param name="address"> The target adress. </param>
    /// <returns> True if the address is in bounds. </returns>
    /// <remarks> May be used from within a mapper script. </remarks>
    bool Contains(MemoryAddress address);

    /// <summary>
    /// Creates a new <see cref="IByteArray"/> from a section of this ones data.
    /// </summary>
    /// <param name="offset"> 
    /// The start of the new chunk. This is relative to the <see cref="Data"/> not <see cref="StartingAddress"/> 
    /// </param>
    /// <param name="length"> The length of the new section. </param>
    /// <returns> The new section. </returns>
    /// <remarks> May be used from within a mapper script. </remarks>
    IByteArray Slice(int offset, int length);

    /// <summary>
    /// Creates new sub-sections of the <see cref="IByteArray"/> by size. If the parent section is 16 bytes long and the
    /// chunk size is 4, then 4 chunks will be returned. <br/>
    /// If the chunk size is not cleanly divisible, then the last chunk will contain the remainder of bytes.
    /// </summary>
    /// <param name="size"> The size of each chunk. </param>
    /// <returns> The array of <see cref="IByteArray"/> memory sections. </returns>
    /// <remarks> May be used from within a mapper script. </remarks>
    IByteArray[] Chunk(int size);

    /// <summary>
    /// Get a single byte from the <see cref="Data"/> at the given offset.
    /// </summary>
    /// <param name="offset"> The offset to read the byte from. Defaults to 0. </param>
    /// <returns> The byte. </returns>
    /// <remarks> Can be used from within a mapper script. </remarks>
    byte get_byte(int offset = 0);

    /// <summary>
    /// Reads 2 bytes of the <see cref="Data"/> from the offset as a little endian unsigned short.
    /// </summary>
    /// <param name="offset"> The offset from which to start readind, inclusive. Defaults to 0. </param>
    /// <returns> The read ushort. </returns>
    /// <remarks> Can be used from within a mapper script. </remarks>
    public ushort get_uint16_le(int offset = 0) => BinaryPrimitives.ReadUInt16LittleEndian(Data.AsSpan().Slice(offset, 2));
    
    /// <summary>
    /// Reads 2 bytes of the <see cref="Data"/> from the offset as a big endian unsigned short.
    /// </summary>
    /// <param name="offset"> The offset from which to start readind, inclusive. Defaults to 0. </param>
    /// <remarks> Can be used from within a mapper script. </remarks>
    /// <returns> The read ushort. </returns>
    public ushort get_uint16_be(int offset = 0) => BinaryPrimitives.ReadUInt16BigEndian(Data.AsSpan().Slice(offset, 2));
    
    /// <summary>
    /// Reads 4 bytes of the <see cref="Data"/> from the offset as a little endian unsigned integer.
    /// </summary>
    /// <param name="offset"> The offset from which to start readind, inclusive. Defaults to 0. </param>
    /// <returns> The read uint. </returns>
    /// <remarks> Can be used from within a mapper script. </remarks>
    public uint get_uint32_le(int offset = 0) => BinaryPrimitives.ReadUInt32LittleEndian(Data.AsSpan().Slice(offset, 4));
    
    /// <summary>
    /// Reads 4 bytes of the <see cref="Data"/> from the offset as a big endian unsigned integer.
    /// </summary>
    /// <param name="offset"> The offset from which to start readind, inclusive. Defaults to 0. </param>
    /// <returns> The read uint. </returns>
    /// <remarks> Can be used from within a mapper script. </remarks>
    public uint get_uint32_be(int offset = 0) => BinaryPrimitives.ReadUInt32BigEndian(Data.AsSpan().Slice(offset, 4));
    
    /// <summary>
    /// Reads 8 bytes of the <see cref="Data"/> from the offset as a little endian unsigned long.
    /// </summary>
    /// <param name="offset"> The offset from which to start readind, inclusive. Defaults to 0. </param>
    /// <returns> The read ulong. </returns>
    /// <remarks> Can be used from within a mapper script. </remarks>
    public ulong get_uint64_le(int offset = 0) => BinaryPrimitives.ReadUInt64LittleEndian(Data.AsSpan().Slice(offset, 8));
    
    /// <summary>
    /// Reads 8 bytes of the <see cref="Data"/> from the offset as a big endian unsigned long.
    /// </summary>
    /// <param name="offset"> The offset from which to start readind, inclusive. Defaults to 0. </param>
    /// <returns> The read ulong. </returns>
    /// <remarks> Can be used from within a mapper script. </remarks>
    public ulong get_uint64_be(int offset = 0) => BinaryPrimitives.ReadUInt64BigEndian(Data.AsSpan().Slice(offset, 8));
}