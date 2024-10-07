namespace PokeAByte.Domain.Interfaces;

/// <summary>
/// Interface to access game memory fetched from an emulator or created by a mapper. <br/>
/// The memory hierarchy is as follows: <br/>
/// - Manager: Contains all memory from the emulator or mapper in <see cref="IMemoryNamespace"/>s <br/>
/// - Namespace: Contains one or more block of memory as defined by either the emulator driver, mapper or mapper script. <br/>
/// - ByteArray: Contains one contigious block of memory.
/// </summary>
/// <remarks>
/// This interface is exposed to mapper javascripts by the <c>__memory</c> object. 
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
