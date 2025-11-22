using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using PokeAByte.Protocol;

namespace PokeAByte.Infrastructure.SharedMemory;

internal static class MacOSShmConstants
{
    public const int O_RDONLY = 0;
    public const int S_IRUSR = 256;
    public const int S_IWUSR = 128;

    public const int PROT_READ = 1;
    public const int MAP_SHARED = 1;
}

/// <summary>
/// Workaround wrapper for .NET trying to use real files on macOS.
/// </summary>
partial class MacOSSharedMemory: ISharedMemory
{
    [LibraryImport("libSystem.dylib", EntryPoint = "shm_open", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int ShmOpen(string path, int oflag, int mode);

    [LibraryImport("libSystem.dylib", EntryPoint = "mmap")]
    private unsafe static partial byte *MemoryMap(byte *addr, nuint length, int prot, int flags, int fildes, nuint offset);

    [LibraryImport("libSystem.dylib", EntryPoint = "close")]
    private static partial void Close(int fd);

    private int _fileSize;
    private bool _disposed;

    private unsafe byte *_mapping;

    private int _fileDescriptor = -1;

    public MacOSSharedMemory(int fileSize)
    {
        _fileSize = fileSize;
        
        // Important to note: This file does not actually exist on the filesystem, so we have to blindly call shm_open
        // and check if it succeeded.
        _fileDescriptor = ShmOpen(
            SharedConstants.GetMmfPath(),
            MacOSShmConstants.O_RDONLY,
            MacOSShmConstants.S_IRUSR | MacOSShmConstants.S_IWUSR
        );
        if (_fileDescriptor < 0) {
            throw new Exception("failed to open macOS MMF: shared memory object could not be opened");
        }

        // We have to mmap this directly. Wrapping it in a SafeFileHandle and passing it to CreateFromFile won't work
        // because it'll complain about not supporting seeking.
        unsafe
        {
            _mapping = MemoryMap(null, (nuint)_fileSize, MacOSShmConstants.PROT_READ, MacOSShmConstants.MAP_SHARED, _fileDescriptor, 0);
        
            // Note: fileSize > 4 MiB will cause mmap to fail due to macOS's weird limits.
            //
            // You can temporarily override this with `sudo sysctl -w kern.sysv.shmmax=<desired size in bytes>`
            if ((IntPtr)_mapping == -1) {
                Close(_fileDescriptor);
                throw new Exception("failed to open macOS MMF: memory mapping failed");
            }
        }
    }

    public void CopyBytesToSpan(ulong offset, Span<byte> destination)
    {
        unsafe {
            var span = new Span<byte>(_mapping + (nuint)offset, destination.Length);
            span.CopyTo(destination);
        }
    }

    public void Dispose()
    {
        if (!_disposed) {
            _disposed = true;

            if (_fileDescriptor < 0) {
                Close(_fileDescriptor);
            }
        }
    }
}
