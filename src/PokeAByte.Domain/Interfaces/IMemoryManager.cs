using System.Buffers.Binary;

namespace PokeAByte.Domain.Interfaces;

/// <summary>
/// Interface to access game memory fetched from an emulator or created by a mapper. <br/>
/// The memory hierarchy is as follows: <br/>
/// - Manager: Contains all memory from the emulator or mapper in <see cref="IMemoryNamespace"/>s <br/>
/// - Namespace: Contains one or more block of memory as defined by either the emulator driver, mapper or mapper script. <br/>
/// - ByteArray: Contains one contigious block of memory.
/// </summary>
/// <remarks>
/// This interface is exposed to mapper javascripts via the <c>__memory</c> object. 
/// </remarks>
public interface IMemoryManager
{
    /// <summary>
    /// Memory namespaces. <br/>
    /// A mapper javascript may add to and retrieve from a custom namepsace. <br/>
    /// Mapper properties without a specified namespace will read from the <see cref="DefaultNamespace"/> 
    /// (key: "default").
    /// </summary>
    Dictionary<string, IMemoryNamespace> Namespaces { get; }

    /// <summary>
    /// The default memory namespace that the emulator memory is written to.
    /// </summary>
    IMemoryNamespace DefaultNamespace { get; }

    /// <summary>
    /// Get a <see cref="IByteArray"/> with a copy of the memory in the target namespace.
    /// </summary>
    /// <param name="area"> The target namespace. <see cref="DefaultNamespace"/> if <see langword="null"/>. </param>
    /// <param name="memoryAddress"> The starting address of memory to copy. </param>
    /// <param name="length"> The number of bytes to copy. </param>
    /// <returns> A <see cref="IByteArray"/> containing the requested data. </returns>
    /// <remarks> 
    /// Implementations of this interface method may throw if the requested address does not exist in the namespace
    /// or if the specified length exceeds the memory bounds.
    /// </remarks>
    IByteArray Get(string? area, MemoryAddress memoryAddress, int length);

    /// <summary>
    /// Get a <see cref="ReadOnlySpan{T}"/> over a number of bytes from starting address in the target namepsace.
    /// </summary>
    /// <param name="area"> The target namespace. <see cref="DefaultNamespace"/> if <see langword="null"/>. </param>
    /// <param name="memoryAddress"> The starting address. </param>
    /// <param name="length"> The number of bytes to read. </param>
    /// <returns> The <see cref="ReadOnlySpan{T}"/>. </returns>
    /// <returns> A readonly span that refers to the target memory, without copying it. </returns>
    /// <remarks>
    /// Implementations of this interface method may throw if the requested address does not exist in the namespace
    /// or if the specified length exceeds the memory bounds. <br/>
    /// Using this method from inside JavaScript is not recommended.
    /// </remarks>
    ReadOnlySpan<byte> GetReadonlyBytes(string? area, MemoryAddress memoryAddress, int length);

    /// <summary>
    /// Write a number of bytes into the target namespace at the specified address.
    /// </summary>
    /// <param name="area"> The ID of the memory namespace. </param>
    /// <param name="memoryAddress"> The starting address of the write. </param>
    /// <param name="data"> The bytes to write into memory. </param>
    /// <remarks>
    /// Implementations of this interface may throw an exception if the <paramref name="data"/> length exceeds the
    /// bounds of the <see cref="IMemoryNamespace"/>.
    /// </remarks>
    void Fill(string area, MemoryAddress memoryAddress, byte[] data);
}

/// <summary>
/// A container for individual memory regions.
/// </summary>
/// <remarks>
/// Different namespaces are allowed to have overlapping addresses, such as the default namespace and a mapper
/// specified namespace both containg data at bytes <c>0x0000</c> through <c>0x0004</c>. 
/// <br/>
/// This interface is exposed to the mapper JavaScript through <see cref="IMemoryManager"/>.
/// </remarks>
public interface IMemoryNamespace
{
    /// <summary>
    /// List of individual memory regions contained in the namespace. <br/>
    /// For instance one fragment may contain <c>0x0000</c> to <c>0x00FF</c>, while the next fragment contains 
    /// <c>0x8000</c> to <c>0x9FFF</c>.
    /// </summary>
    /// <remarks>
    /// Using this property from within mapper JavaScript should be avoided, as fragments may be different 
    /// between different emulator drivers.
    /// </remarks>
    IList<IByteArray> Fragments { get; }

    /// <summary>
    /// Write data starting at the target address.
    /// </summary>
    /// <param name="memoryAddress"> The target address. </param>
    /// <param name="data"> The array of bytes to write into the namespace. </param>
    public void Fill(MemoryAddress memoryAddress, byte[] data);

    /// <summary>
    /// Check whether a given address is contained within this namespace.
    /// </summary>
    /// <param name="memoryAddress"> The address to check. </param>
    /// <returns>
    /// <see langword="true"/> if the namespace contains the address. Otherwise <see langword="false"/>.
    /// </returns>
    bool Contains(MemoryAddress memoryAddress);

    /// <summary>
    /// Get a <see cref="ReadOnlySpan{T}"/> over the target memory in this namepsace.
    /// </summary>
    /// <param name="memoryAddress"> Starting address of the target memory. </param>
    /// <param name="length"> Lenght of the target memory. </param>
    /// <returns> The <see cref="ReadOnlySpan{T}"/>. </returns>
    /// <remarks> 
    /// Implementations of this interface method may throw if the memory address or length are out of bounds
    /// of the namespace. <br/>
    /// Using this from inside mapper JavaScript is not recommended.
    /// </remarks>
    ReadOnlySpan<byte> GetReadonlyBytes(MemoryAddress memoryAddress, int length);

    /// <summary>
    /// Read an individual byte from the target address.
    /// </summary>
    /// <param name="memoryAddress"> The target address. </param>
    /// <returns> The byte value. </returns>
    /// <remarks>
    /// Implementations of this interface method may throw if the memory address is not contained within the namespace.
    /// </remarks>
    byte get_byte(MemoryAddress memoryAddress);

    /// <summary>
    /// Get a <see cref="IByteArray" /> instance with a copy of the specified memory data.
    /// </summary>
    /// <param name="memoryAddress"> Starting address of the target memory. </param>
    /// <param name="length"> Lenght of the target memory. </param>
    /// <returns> A <see cref="IByteArray"/> instance. </returns>
    IByteArray get_bytes(MemoryAddress memoryAddress, int length);

    /// <summary>
    /// Reads 2 bytes starting at the target address as a little endian unsigned short.
    /// </summary>
    /// <param name="memoryAddress"> The target address. </param>
    /// <returns>
    /// The <see cref="ushort"/>.
    /// </returns>
    /// <remarks>
    /// This may throw an exception when the <paramref name="memoryAddress"/> is not contained in the namespace
    /// or if the length of 2 exceeds the bounds of the fragment.
    /// </remarks>
    public ushort get_uint16_le(MemoryAddress memoryAddress)
    {
        var bytes = GetReadonlyBytes(memoryAddress, 2);
        return BinaryPrimitives.ReadUInt16LittleEndian(bytes);
    }

    /// <summary>
    /// Reads 2 bytes starting at the target address as a big endian unsigned short.
    /// </summary>
    /// <param name="memoryAddress"> The target address. </param>
    /// <returns>
    /// The <see cref="ushort"/>.
    /// </returns>
    /// <remarks>
    /// This may throw an exception when the <paramref name="memoryAddress"/> is not contained in the namespace
    /// or if the length of 2 exceeds the bounds of the fragment.
    /// </remarks>
    public ushort get_uint16_be(MemoryAddress memoryAddress)
    {
        var bytes = GetReadonlyBytes(memoryAddress, 2);
        return BinaryPrimitives.ReadUInt16BigEndian(bytes);
    }

    /// <summary>
    /// Reads 4 bytes starting at the target address as a little endian unsigned integer.
    /// </summary>
    /// <param name="memoryAddress"> The target address. </param>
    /// <returns>
    /// The <see cref="uint"/>.
    /// </returns>
    /// <remarks>
    /// This may throw an exception when the <paramref name="memoryAddress"/> is not contained in the namespace
    /// or if the length of 4 exceeds the bounds of the fragment.
    /// </remarks>
    public uint get_uint32_le(MemoryAddress memoryAddress)
    {
        var bytes = GetReadonlyBytes(memoryAddress, 4);
        return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
    }

    /// <summary>
    /// Reads 4 bytes starting at the target address as a big endian unsigned integer.
    /// </summary>
    /// <param name="memoryAddress"> The target address. </param>
    /// <returns>
    /// The <see cref="uint"/>.
    /// </returns>
    /// <remarks>
    /// This may throw an exception when the <paramref name="memoryAddress"/> is not contained in the namespace
    /// or if the length of 4 exceeds the bounds of the fragment.
    /// </remarks>
    public uint get_uint32_be(MemoryAddress memoryAddress)
    {
        var bytes = GetReadonlyBytes(memoryAddress, 4);
        return BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }

    /// <summary>
    /// Reads 8 bytes starting at the target address as a little endian unsigned long.
    /// </summary>
    /// <param name="memoryAddress"> The target address. </param>
    /// <returns>
    /// The <see cref="ulong"/>.
    /// </returns>
    /// <remarks>
    /// This may throw an exception when the <paramref name="memoryAddress"/> is not contained in the namespace
    /// or if the length of 8 exceeds the bounds of the fragment.
    /// </remarks>
    public ulong get_uint64_le(MemoryAddress memoryAddress) => (ulong)((get_uint32_le(memoryAddress + 0) << 0) | (get_uint32_le(memoryAddress + 4) << 32));

    /// <summary>
    /// Reads 8 bytes starting at the target address as a big endian unsigned integer.
    /// </summary>
    /// <param name="memoryAddress"> The target address. </param>
    /// <returns>
    /// The <see cref="uint"/>.
    /// </returns>
    /// <remarks>
    /// This may throw an exception when the <paramref name="memoryAddress"/> is not contained in the namespace
    /// or if the length of 8 exceeds the bounds of the fragment.
    /// </remarks>
    public ulong get_uint64_be(MemoryAddress memoryAddress) => (ulong)((get_uint32_be(memoryAddress + 0) << 32) | (get_uint32_be(memoryAddress + 4) << 0));
}
