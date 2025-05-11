using System;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Emulation.Common;

namespace PokeAByte.Protocol.BizHawk;

internal struct DomainReadInstruction {
    internal string Domain;
    internal long RelativeStart;
    internal long RelativeEnd;

    /// <summary>
    /// Position of the block in the MFF (first byte).
    /// </summary>
    internal uint TransferPosition; 
}

[ExternalTool("Emulator Data Protocol Server")]
public sealed class EDPSForm : Form, IExternalToolForm, IDisposable
{
    public ApiContainer? APIs { get; set; }

    [RequiredService]
    public IMemoryDomains MemoryDomains { get; set; } = null!;

    private readonly Label MainLabel = new()
    {
        Text = "Loading...",
        Height = 50,
        TextAlign = ContentAlignment.MiddleCenter,
        Dock = DockStyle.Top
    };

    public bool IsActive { get; }

    public bool IsLoaded { get; }

    private PlatformConstants.PlatformEntry? _platform;
    private string System = string.Empty;

    private EmulatorProtocolServer? _udpServer;
    private ReadBlock[]? _blocks;
    private MemoryMappedViewAccessor? _dataAccessor;
    private byte[] _writeBuffer = new byte[0];
    private DomainReadInstruction[]? _readInstructions;

    public EDPSForm()
    {
        base.Text = "Emulator Data Protocol Server";
        ShowInTaskbar = false;

        ClientSize = new(300, 60);
        SuspendLayout();

        Controls.Add(MainLabel);

        ResumeLayout(performLayout: false);
        PerformLayout();
        StartServer();

        Closing += (sender, args) =>
        {
            Cleanup();
        };
        IsLoaded = true;
        IsActive = true;
    }

    private void StartServer()
    {
        _udpServer = new EmulatorProtocolServer();
        _udpServer.OnWrite = WriteToMemory;
        _udpServer.OnSetup = Setup;
        _udpServer.Start();
    }

    private void Cleanup()
    {
        MainLabel.Text = $"Waiting for Client...";
        _udpServer?.Dispose();
        _udpServer = null;
        _dataAccessor?.Dispose();
        _dataAccessor = null;
        _blocks = [];
        _readInstructions = null;
        if (File.Exists("/dev/shm/EDPS_MemoryData.bin"))
        {
            File.Delete("/dev/shm/EDPS_MemoryData.bin");
        }
    }

    private MemoryMappedViewAccessor GetMMFAccessor(int size)
    {
        MemoryMappedViewAccessor accessor;
        MemoryMappedFile memoryMappedFile;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            memoryMappedFile = MemoryMappedFile.CreateOrOpen("EDPS_MemoryData.bin", size, MemoryMappedFileAccess.ReadWrite);
        }
        else
        {
            if (File.Exists("/dev/shm/EDPS_MemoryData.bin"))
            {
                File.Delete("/dev/shm/EDPS_MemoryData.bin");
            }
            memoryMappedFile = MemoryMappedFile.CreateFromFile(
                "/dev/shm/EDPS_MemoryData.bin",
                FileMode.OpenOrCreate,
                null,
                size,
                MemoryMappedFileAccess.ReadWrite
            );
        }
        accessor = memoryMappedFile.CreateViewAccessor();
        memoryMappedFile.Dispose();
        return accessor;
    }

    private void Setup(SetupInstruction instruction)
    {
        MainLabel.Text = $"Setting up data connection";
        int totalSize = 0;
        _blocks = new ReadBlock[instruction.BlockCount];
        for (int i = 0; i < instruction.BlockCount; i++)
        {
            totalSize += instruction.Data[i].Length;
            _blocks[i] = instruction.Data[i];
        }
        if (totalSize == 0)
        {
            throw new InvalidDataException("Setup instruction came with invalid block sizes. ");
        }
        _dataAccessor = this.GetMMFAccessor(totalSize);
        _writeBuffer = new byte[totalSize];
        MainLabel.Text = $"Providing memory data ({totalSize} bytes) to client...";
    }

    private void WriteToMemory(WriteInstruction instruction)
    {
        if (instruction.Data.Length == 0 || APIs == null)
        {
            Console.WriteLine("Zero length write instruction. Abort.");
            return;
        }
        try
        {
            APIs.Memory.WriteByteRange(instruction.Address, instruction.Data);
        }
        catch (Exception)
        {
            // todo: error handling?
        }
    }

    public void Restart()
    {
        Cleanup();
        StartServer();
        System = APIs?.Emulation.GetGameInfo()?.System ?? string.Empty;
        _platform = PlatformConstants.Platforms.SingleOrDefault(x => x.SystemId == System);

        if (string.IsNullOrWhiteSpace(System))
        {
            MainLabel.Text = "No game is loaded, doing nothing.";
        }
        else
        {
            MainLabel.Text = $"Waiting for client...";
        }
    }


    public bool AskSaveChanges() => true;

    private void UpdateAfter()
    {
        if (_dataAccessor == null || _blocks == null || APIs == null || _platform == null)
        {
            return;
        }
        if (_readInstructions == null) {
            _readInstructions = new DomainReadInstruction[_blocks.Length];
            for (int i = 0; i < _blocks.Length; i++)
            {
                var block = _blocks[i];
                var entry = _platform.Domains.First(x => x.Start <= block.GameAddress && x.End >= block.GameAddress + block.Length);
                var address = block.GameAddress - entry.Start;
                _readInstructions[i] = new DomainReadInstruction {
                    Domain = entry.DomainId,
                    TransferPosition = block.Position,
                    RelativeStart = address,
                    RelativeEnd = address + block.Length,
                };
            }
        }

        foreach (var instruction in _readInstructions)
        {
            try
            {
                var domain = MemoryDomains[instruction.Domain];
                if (domain != null) {
                    int blockPosition = (int)instruction.TransferPosition;
                    for(long i = instruction.RelativeStart; i <= instruction.RelativeEnd; i++) {
                        _writeBuffer[blockPosition++] = domain.PeekByte(i);
                    }
                }
            }
            catch (Exception ex)
            {
                MainLabel.Text = $"Error reading {instruction.RelativeStart:x2} in '{instruction.Domain}': {ex.Message}";
            }
        }
        _dataAccessor.WriteArray(0, _writeBuffer, 0, _writeBuffer.Length);
    }

    public void UpdateValues(ToolFormUpdateType type)
    {
        if (type == ToolFormUpdateType.PostFrame)
        {
            UpdateAfter();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.Cleanup();
        }
        base.Dispose(disposing);
    }
}


#region PlatformConstants
internal static class PlatformConstants
{
    internal class PlatformEntry
    {
        internal string SystemId { get; }
        internal int FrameSkipDefault { get; }
        internal DomainLayout[] Domains { get; }

        public PlatformEntry(string systemId, DomainLayout[] domains, int frameSkipDefault = 0)
        {
            SystemId = systemId;
            FrameSkipDefault = frameSkipDefault;
            Domains = domains;
        }
    }

    internal readonly struct DomainLayout
    {
        public readonly string DomainId;
        public readonly long Start;
        public readonly int Length;
        public readonly long End;

        public DomainLayout(string domain, long start, int length)
        {
            DomainId = domain;
            Start = start;
            Length = length;
            End = start + length;
        }
    }

    public static readonly PlatformEntry[] Platforms =
    [
        new PlatformEntry(
            "NES",
            [ new DomainLayout("RAM", 0x00, 0x800) ]
        ),
        new PlatformEntry(
            "SNES",
            [new DomainLayout("WRAM",0x7E0000, 0x10000)]
        ),
        new PlatformEntry(
            "GB",
            [
                new DomainLayout("WRAM", 0xC000, 0x2000),
                new DomainLayout("VRAM", 0x8000, 0x1FFF),
                new DomainLayout("HRAM", 0xFF80, 0x7E)
            ]
        ),
        new PlatformEntry(
            "GBC",
            [
                new DomainLayout("WRAM", 0xC000, 0x2000),
                new DomainLayout("VRAM", 0x8000, 0x1FFF),
                new DomainLayout("HRAM", 0xFF80, 0x7E),
            ]
        ),
        new PlatformEntry(
            "GBA",
            [
                new DomainLayout("EWRAM", 0x02000000, 0x00040000),
                new DomainLayout("IWRAM", 0x03000000, 0x00008000),
            ]
        ),
        new PlatformEntry(
            "NDS",
            [ new DomainLayout("Main RAM", 0x2000000, 0x400000) ],
            15
        )
    ];
}
#endregion