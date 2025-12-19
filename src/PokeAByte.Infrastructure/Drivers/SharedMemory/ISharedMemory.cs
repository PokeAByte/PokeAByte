using System.Runtime.InteropServices;

namespace PokeAByte.Infrastructure.SharedMemory;

public interface ISharedMemory
{
    void CopyBytesToSpan(ulong offset, Span<byte> destination);
    void Dispose();

    static ISharedMemory Get(int fileSize)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            return new MacOSSharedMemory(fileSize);
        } else {
            return new MemoryMappedFileSharedMemory(fileSize);
        }
    }
}
