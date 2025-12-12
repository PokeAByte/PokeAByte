using System;
using System.Collections.Generic;
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
    private Label _mainLabel;
    private PlatformEntry _platform;
    private MemoryMappedViewAccessor _dataAccessor;
    private DomainReadInstruction[] _readInstructions;
    private int _frameSkip;
    private int _skippedFrames;
    private MemoryMappedFile _memoryMappedFile;
    private byte[] _writeBuffer;
    private Queue<WriteInstruction> _writeQueue = new();

    internal GameDataProcessor(
        PlatformEntry platform,
        SetupInstruction setup,
        Label mainLabel)
    {
        _mainLabel = mainLabel;
        _platform = platform;

        int totalSize = 0;
        var blocks = new ReadBlock[setup.BlockCount];
        _readInstructions = new DomainReadInstruction[blocks.Length];
        _frameSkip = setup.FrameSkip == -1 ? _platform.FrameSkipDefault : setup.FrameSkip;

        for (int i = 0; i < setup.BlockCount; i++)
        {
            var readBlock = setup.Data[i];
            DomainLayout? entry = _platform.Domains.FirstOrDefault(x => x.Start <= readBlock.GameAddress && x.End >= readBlock.GameAddress + readBlock.Length - 1);
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
                RelativeEnd = address + readBlock.Length
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
        _memoryMappedFile = CreateMemoryMappedFile(totalSize);
        _dataAccessor = _memoryMappedFile.CreateViewAccessor();
        _writeBuffer = new byte[totalSize];
        mainLabel.Text = $"Providing memory data ({totalSize} bytes) to client...";
    }

    /// <summary>
    /// Write the byte array into the memory mapped file.
    /// </summary>
    /// <param name="source"> The bytes to write. </param>
    /// <param name="position"> The position in the MMF where the write starts. </param>
    /// <param name="length"> How many bytes from the source to write into the MMF. </param>
    private unsafe void UpdateMemoryMappedFile(byte[] source, int position, int length)
    {
        try
        {
            byte* destination = null;
            _dataAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref destination);
            fixed (byte* sourcePointer = source)
            {
                Buffer.MemoryCopy(sourcePointer, destination + position, length, length);
            }
        }
        finally
        {
            _dataAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
        }
    }

    /// <summary>
    /// Execute the given write instruction, updating the games memory.
    /// </summary>
    /// <param name="instruction"> The write instruction to execute. </param>
    /// <param name="domains"> The BizHawk memory domains to be used for memory access. </param>
    private void WriteToGameMemory(WriteInstruction instruction, IMemoryDomains domains)
    {
        DomainLayout? layout = _platform.Domains.FirstOrDefault(
            x => x.Start <= instruction.Address && x.End >= instruction.Address + instruction.Data.Length
        );
        if (layout == null)
        {
            return;
        }
        var domain = domains[layout.Value.DomainId];
        if (domain == null)
        {
            return;
        }
        if (instruction.Data.Length != 0 && layout != null)
        {
            domain.Enter();
            for (int i = 0; i < instruction.Data.Length; i++)
            {
                domain.PokeByte(instruction.Address + i - layout.Value.Start, instruction.Data[i]);
            }
            domain.Exit();
        }
    }

    private MemoryMappedFile CreateMemoryMappedFile(int size)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return MemoryMappedFile.CreateOrOpen(
                SharedConstants.MemoryMappedFileName,
                size,
                MemoryMappedFileAccess.ReadWrite
            );
        }
        else
        {
            if (File.Exists(SharedConstants.GetMmfPath()))
            {
                File.Delete(SharedConstants.GetMmfPath());
            }
            return MemoryMappedFile.CreateFromFile(
                SharedConstants.GetMmfPath(),
                FileMode.OpenOrCreate,
                null,
                size,
                MemoryMappedFileAccess.ReadWrite
            );
        }
    }

    /// <summary>
    /// Write memory back into the emulator if a write has been queued and read emulator memory and update the memory
    /// mapped file.
    /// </summary>
    /// <param name="memoryDomains"> The BizHawk memory domains to be used for memory access. </param>
    internal void UpdateGameMemory(IMemoryDomains memoryDomains)
    {
        while (_writeQueue.Count > 0)
        {
            WriteToGameMemory(_writeQueue.Dequeue(), memoryDomains);
        }

        if (_skippedFrames > _frameSkip)
        {
            _skippedFrames = 0;
        }
        else
        {
            _skippedFrames++;
            return;
        }

        foreach (DomainReadInstruction instruction in _readInstructions)
        {
            try
            {
                if (instruction.RelativeStart == instruction.RelativeEnd)
                {
                    continue;
                }
                var domain = memoryDomains[instruction.Domain];
                if (domain != null)
                {
                    var length = instruction.RelativeEnd - instruction.RelativeStart;
                    domain.BulkPeekByte(instruction.RelativeStart.RangeToExclusive(instruction.RelativeEnd), _writeBuffer);
                    UpdateMemoryMappedFile(_writeBuffer, (int)instruction.TransferPosition, (int)length);
                }
            }
            catch (Exception ex)
            {
                _mainLabel.Text = $"Error reading {instruction.RelativeStart:x2} in '{instruction.Domain}': {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Add a write instruction to the queue. The instruction will be executed the next time 
    /// <see cref="UpdateGameMemory(IMemoryDomains)"/> is called, i.e. after the current emulation frame is done.
    /// </summary>
    /// <param name="instruction"> The instruction to queue.</param>
    internal void QueueWrite(WriteInstruction instruction)
    {
        this._writeQueue.Enqueue(instruction);
    }

    public void Dispose()
    {
        this._dataAccessor.Dispose();
        this._memoryMappedFile.Dispose();
        if (File.Exists(SharedConstants.GetMmfPath()))
        {
            File.Delete(SharedConstants.GetMmfPath());
        }
    }
}
