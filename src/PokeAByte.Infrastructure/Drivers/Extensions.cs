using System.IO.MemoryMappedFiles;
using PokeAByte.Domain;

namespace PokeAByte.Infrastructure;

public static class MemoryMappedViewAccessorExtensions
{
    /// <summary>
    /// Copy a length of bytes from the memory mapped file view to the destination span.
    /// </summary>
    /// <param name="accessor"> The view accessor to copy from. </param>
    /// <param name="offset"> The position at which to start copying. </param>
    /// <param name="destination"> The destination span. </param>
    /// <exception cref="ArgumentException"> Thrown if offset + destination length exceed the MFF capacity. </exception>
    public static void CopyBytesToSpan(this MemoryMappedViewAccessor accessor, ulong offset, Span<byte> destination)
    {
        unsafe
        {
            try
            {
                byte* pointer = null;
                if (accessor.SafeMemoryMappedViewHandle.ByteLength < (offset + (ulong)destination.Length))
                {
                    throw new PokeAByteException("Driver tried to read memory out of bounds of the memory mapped file.");
                }
                accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
                var span = new Span<byte>(pointer + offset, destination.Length);
                span.CopyTo(destination);
            }
            finally
            {
                accessor.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }
    }
}