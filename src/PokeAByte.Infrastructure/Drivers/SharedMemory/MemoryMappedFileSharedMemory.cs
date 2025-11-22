using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using PokeAByte.Protocol;

namespace PokeAByte.Infrastructure.SharedMemory;

class MemoryMappedFileSharedMemory: ISharedMemory
{
    private int _fileSize;
    private bool _disposed;

    private MemoryMappedFile _mmfData;
    private MemoryMappedViewAccessor _mmfDataView;

    public MemoryMappedFileSharedMemory(int fileSize)
    {
        _fileSize = fileSize;
        _mmfData = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? MemoryMappedFile.OpenExisting(SharedConstants.MemoryMappedFileName, MemoryMappedFileRights.Read)
            : MemoryMappedFile.CreateFromFile(
                SharedConstants.GetMmfPath(),
                FileMode.Open,
                null,
                _fileSize,
                MemoryMappedFileAccess.Read
            );
        _mmfDataView = _mmfData.CreateViewAccessor(0, _fileSize, MemoryMappedFileAccess.Read);
    }

    public void CopyBytesToSpan(ulong offset, Span<byte> destination)
    {
        _mmfDataView.CopyBytesToSpan(offset, destination);
    }

    public void Dispose()
    {
        if (!_disposed) {
            _disposed = true;
            _mmfData.Dispose();
            _mmfDataView.Dispose();
        }
    }
}
