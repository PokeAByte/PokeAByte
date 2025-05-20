using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BizHawk.Common;
using BizHawk.Emulation.Common;
using PokeAByte.Protocol.BizHawk.PlatformData;

namespace PokeAByte.Protocol.BizHawk;

internal class GameDataProcessor : IDisposable
{
    private IMemoryDomains _memoryDomains { get; set; } = null!;

    private Label _mainLabel;
    private PlatformEntry _platform;
    private MemoryMappedViewAccessor _dataAccessor;
    private byte[] _writeBuffer;
    private DomainReadInstruction[] _readInstructions;
    private int _frameSkip;
    private int _skippedFrames;
    private MemoryMappedFile _memoryMappedFile;

    private byte[] DataBuffer { get; } = new byte[4 * 1024 * 1024];


    internal GameDataProcessor(
        IMemoryDomains memoryDomains,
        PlatformEntry platform,
        SetupInstruction setup,
        Label mainLabel
    )
    {
        _mainLabel = mainLabel;
        _memoryDomains = memoryDomains;
        _platform = platform;

        int totalSize = 0;
        var blocks = new ReadBlock[setup.BlockCount];
        _readInstructions = new DomainReadInstruction[blocks.Length];
        _frameSkip = setup.FrameSkip == -1 ? _platform.FrameSkipDefault : setup.FrameSkip;

        for (int i = 0; i < setup.BlockCount; i++)
        {
            var readBlock = setup.Data[i];
            DomainLayout? entry = _platform.Domains.FirstOrDefault(x => x.Start <= readBlock.GameAddress && x.End >= readBlock.GameAddress + readBlock.Length);
            if (entry == null)
            {
                continue;
            }
            var address = readBlock.GameAddress - entry.Value.Start;
            _readInstructions[i] = new DomainReadInstruction
            {
                Domain = entry.Value.DomainId,
                TransferPosition = readBlock.Position,
                RelativeStart = address,
                RelativeEnd = address + readBlock.Length - 1,
            };
        }
        for (int i = 0; i < setup.BlockCount; i++)
        {
            totalSize += setup.Data[i].Length;
            blocks[i] = setup.Data[i];
        }
        if (totalSize == 0)
        {
            throw new InvalidDataException("Setup instruction came with invalid block sizes. ");
        }
        GetMMFAccessor(totalSize);
        if (_memoryMappedFile == null)
        {
            _memoryMappedFile = null!;
        }
        if (_dataAccessor == null)
        {
            _dataAccessor = null!;
        }
        _writeBuffer = new byte[totalSize];
        mainLabel.Text = $"Providing memory data ({totalSize} bytes) to client...";
    }

    private void GetMMFAccessor(int size)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _memoryMappedFile = MemoryMappedFile.CreateOrOpen(
                SharedConstants.MemoryMappedFileName,
                size,
                MemoryMappedFileAccess.ReadWrite
            );
        }
        else
        {
            if (File.Exists($"/dev/shm/{SharedConstants.MemoryMappedFileName}"))
            {
                File.Delete($"/dev/shm/{SharedConstants.MemoryMappedFileName}");
            }
            _memoryMappedFile = MemoryMappedFile.CreateFromFile(
                $"/dev/shm/{SharedConstants.MemoryMappedFileName}",
                FileMode.OpenOrCreate,
                null,
                size,
                MemoryMappedFileAccess.ReadWrite
            );
        }
        _dataAccessor = _memoryMappedFile.CreateViewAccessor();
    }

    public void Update()
    {
        if (_skippedFrames > _frameSkip)
        {
            _skippedFrames = 0;
        }
        else
        {
            _skippedFrames++;
            return;
        }
        DomainReadInstruction instruction = _readInstructions[0];
        try
        {
            for (int i = 0; i < _readInstructions.Length; i++)
            {
                instruction = _readInstructions[i];
                if (instruction.RelativeStart == instruction.RelativeEnd)
                {
                    continue;
                }
                var domain = _memoryDomains[instruction.Domain];
                if (domain != null)
                {
                    var length = instruction.RelativeEnd - instruction.RelativeStart;
                    domain.BulkPeekByte(instruction.RelativeStart.RangeToExclusive(instruction.RelativeEnd), DataBuffer);
                    Buffer.BlockCopy(DataBuffer, 0, _writeBuffer, (int)instruction.TransferPosition, (int)length);
                }
            }
        }
        catch (Exception ex)
        {
            _mainLabel.Text = $"Error reading {instruction.RelativeStart:x2} in '{instruction.Domain}': {ex.Message}";
        }
        _dataAccessor.WriteArray(0, _writeBuffer, 0, _writeBuffer.Length);
    }

    public void Dispose()
    {
        this._dataAccessor.Dispose();
        this._memoryMappedFile.Dispose();
        if (File.Exists($"/dev/shm/{SharedConstants.MemoryMappedFileName}"))
        {
            File.Delete($"/dev/shm/{SharedConstants.MemoryMappedFileName}");
        }
    }
}
