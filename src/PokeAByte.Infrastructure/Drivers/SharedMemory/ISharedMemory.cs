using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using PokeAByte.Protocol;

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
