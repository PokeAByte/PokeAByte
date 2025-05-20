using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Common;
using BizHawk.Emulation.Common;

namespace PokeAByte.Integrations.BizHawk;

[ExternalTool("Poke-A-Byte Integration")]
public sealed class PokeAByteIntegrationForm : Form, IExternalToolForm, IDisposable
{
    private static string LINUX_METADATA_FILE = "/dev/shm/POKEABYTE_BIZHAWK.bin";
    private static string LINUX_DATA_FILE = "/dev/shm/POKEABYTE_BIZHAWK_DATA.bin";
    public ApiContainer? APIs { get; set; }

    [RequiredService]
    public IMemoryDomains? MemoryDomains { get; set; }

    private readonly Label MainLabel = new() { Text = "Loading...", Height = 50, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top };

    private MemoryMappedFile? MetaDataMMF;
    private MemoryMappedViewAccessor? Metadata_Accessor;

    private MemoryMappedFile? Data_MMF;
    private MemoryMappedViewAccessor? Data_Accessor;

    private byte[] DataBuffer { get; } = new byte[SharedPlatformConstants.BIZHAWK_DATA_PACKET_SIZE];
    public bool IsActive { get; private set; } = true;
    public bool IsLoaded => true;

    private string System = string.Empty;

    private SharedPlatformConstants.PlatformEntry? Platform = null;
    private int? FrameSkip = null;
    private NamedPipeServer? _namedPipeServer = null;

    public PokeAByteIntegrationForm()
    {
        base.Text = "Poke-A-Byte Integration";
        ShowInTaskbar = false;

        ClientSize = new(300, 60);
        SuspendLayout();

        Controls.Add(MainLabel);

        ResumeLayout(performLayout: false);
        PerformLayout();

        _namedPipeServer = new NamedPipeServer();
        _namedPipeServer.ClientDataHandler += ReadFromClient;
        _namedPipeServer.StartServer("BizHawk_Named_Pipe");
        Closing += (sender, args) =>
        {
            IsActive = false;
            _namedPipeServer?.Dispose();
            _namedPipeServer = null;
        };
    }

    private MemoryMappedFile CreateMMF(string name, int size)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return MemoryMappedFile.CreateOrOpen(name, size, MemoryMappedFileAccess.ReadWrite);
        }
        else
        {
            if (File.Exists("/dev/shm/" + name))
            {
                File.Delete("/dev/shm/" + name);
            }
            return MemoryMappedFile.CreateFromFile(
                "/dev/shm/" + name,
                FileMode.OpenOrCreate,
                null,
                size,
                MemoryMappedFileAccess.ReadWrite
            );
        }
    }

    private void CreateMemoryMappedFiles()
    {
        if (this.Platform == null)
        {
            return;
        }
        MetaDataMMF = CreateMMF(
            "POKEABYTE_BIZHAWK.bin",
            SharedPlatformConstants.BIZHAWK_METADATA_PACKET_SIZE
        );
        Metadata_Accessor = MetaDataMMF.CreateViewAccessor();

        Data_MMF = CreateMMF(
            "POKEABYTE_BIZHAWK_DATA.bin",
            SharedPlatformConstants.BIZHAWK_DATA_PACKET_SIZE
        );
        Data_Accessor = Data_MMF.CreateViewAccessor();
    }

    private void ReadFromClient(MemoryContract? clientData)
    {
        if (clientData?.Data is null || string.IsNullOrWhiteSpace(clientData.BizHawkIdentifier))
            return;
        var memoryDomain = MemoryDomains?[clientData.BizHawkIdentifier]
            ?? throw new Exception($"Memory domain not found.");
        memoryDomain.Enter();
        for (int i = 0; i < clientData.Data.Length; i++)
        {
            //0x244EC
            memoryDomain.PokeByte(clientData.MemoryAddressStart + i, clientData.Data[i]);
        }
        memoryDomain.Exit();
    }

    public void Restart()
    {
        System = APIs?.Emulation.GetGameInfo()?.System ?? string.Empty;
        Platform = SharedPlatformConstants.Information.SingleOrDefault(x => x.BizhawkIdentifier == System);
        CreateMemoryMappedFiles();
        var data = new byte[SharedPlatformConstants.BIZHAWK_METADATA_PACKET_SIZE];

        data[0] = 0x00;
        data[1] = SharedPlatformConstants.BIZHAWK_INTEGRATION_VERSION;

        Array.Copy(Encoding.UTF8.GetBytes(System), 0, data, 2, System.Length);

        Metadata_Accessor?.WriteArray(0, data, 0, data.Length);

        if (string.IsNullOrWhiteSpace(System))
        {
            MainLabel.Text = "No game is loaded, doing nothing.";
        }
        else if (Platform == null)
        {
            MainLabel.Text = $"{System} is not yet supported.";
        }
        else
        {
            FrameSkip = Platform.FrameSkipDefault;

            MainLabel.Text = $"Sending {System} data to PokeAByte...";
        }
    }

    private void UpdateAfter()
    {
        try
        {
            if (Platform == null) { return; }

            if (Platform.FrameSkipDefault != null)
            {
                FrameSkip -= 1;

                if (FrameSkip != 0) { return; }
            }

            foreach (var entry in Platform.MemoryLayout)
            {
                try
                {
                    var memoryDomain = MemoryDomains?[entry.BizhawkIdentifier] ?? throw new Exception($"Memory domain not found.");
                    memoryDomain.BulkPeekByte(0x00L.RangeToExclusive(entry.Length), DataBuffer);
                    Data_Accessor?.WriteArray(entry.CustomPacketTransmitPosition, DataBuffer, 0, entry.Length);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to read memory domain {entry.BizhawkIdentifier}. {ex.Message}", ex);
                }
            }
            if (FrameSkip == 0)
            {
                FrameSkip = Platform.FrameSkipDefault;
            }
        }
        catch (Exception ex)
        {
            MainLabel.Text = $"Error: {ex.Message}";
        }
    }

    public void UpdateValues(ToolFormUpdateType type)
    {
        if (type == ToolFormUpdateType.PostFrame)
        {
            UpdateAfter();
        }
    }

    public bool AskSaveChanges() => true;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Data_MMF?.Dispose();
            Data_MMF = null;
            Data_Accessor?.Dispose();
            Data_Accessor = null;

            MetaDataMMF?.Dispose();
            MetaDataMMF = null;
            Metadata_Accessor?.Dispose();
            Metadata_Accessor = null;

            if (File.Exists(LINUX_METADATA_FILE))
            {
                File.Delete(LINUX_METADATA_FILE);
            }
            if (File.Exists(LINUX_DATA_FILE))
            {
                File.Delete(LINUX_DATA_FILE);
            }
        }
        base.Dispose(disposing);
    }
}

#region PlatformConstants
public static class SharedPlatformConstants
{
    public record PlatformEntry
    {
        public bool IsBigEndian { get; set; } = false;
        public bool IsLittleEndian => IsBigEndian == false;
        public string BizhawkIdentifier { get; set; } = string.Empty;
        public int? FrameSkipDefault { get; set; } = null;

        public PlatformMemoryLayoutEntry[] MemoryLayout { get; set; } = Array.Empty<PlatformMemoryLayoutEntry>();
    }

    public record PlatformMemoryLayoutEntry
    {
        public string BizhawkIdentifier { get; set; } = string.Empty;
        public int CustomPacketTransmitPosition { get; set; } = 0;
        public int Length { get; set; } = 0;

        public long PhysicalStartingAddress = 0x00;
        public long PhysicalEndingAddress => PhysicalStartingAddress + Length;
    }

    public const int BIZHAWK_INTEGRATION_VERSION = 0x00;
    public const int BIZHAWK_METADATA_PACKET_SIZE = 32;
    public const int BIZHAWK_ROM_PACKET_SIZE = 0x200000 * 2;
    public const int BIZHAWK_DATA_PACKET_SIZE = 4 * 1024 * 1024;

    public static readonly IEnumerable<PlatformEntry> Information = new List<PlatformEntry>()
    {
        new PlatformEntry
        {
            IsBigEndian = true,
            BizhawkIdentifier = "NES",
            MemoryLayout = new PlatformMemoryLayoutEntry[]
            {
                new PlatformMemoryLayoutEntry
                {
                    BizhawkIdentifier = "RAM",
                    CustomPacketTransmitPosition = 0,
                    PhysicalStartingAddress = 0x00,
                    Length = 0x800
                }
            }
        },
        new PlatformEntry
        {
            IsBigEndian = false,
            BizhawkIdentifier = "SNES",
            MemoryLayout = new PlatformMemoryLayoutEntry[]
            {
                new PlatformMemoryLayoutEntry
                {
                    BizhawkIdentifier = "WRAM",
                    CustomPacketTransmitPosition = 0,
                    PhysicalStartingAddress = 0x7E0000,
                    Length = 0x10000
                }
            }
        },
        new PlatformEntry()
        {
            IsBigEndian = false,
            BizhawkIdentifier = "GB",
            MemoryLayout = new PlatformMemoryLayoutEntry[]
            {
                new PlatformMemoryLayoutEntry {
                    BizhawkIdentifier = "WRAM",
                    CustomPacketTransmitPosition = 0,
                    PhysicalStartingAddress = 0xC000,
                    Length = 0x2000
                },
                new PlatformMemoryLayoutEntry {
                    BizhawkIdentifier = "VRAM",
                    CustomPacketTransmitPosition = 0x2000 + 1,
                    PhysicalStartingAddress = 0x8000,
                    Length = 0x1FFF
                },
                new PlatformMemoryLayoutEntry {
                    BizhawkIdentifier = "HRAM",
                    CustomPacketTransmitPosition = 0x1000 + 0x1FFF + 1,
                    PhysicalStartingAddress = 0xFF80,
                    Length = 0x7E
                }
            }
        },
        new PlatformEntry()
        {
            IsBigEndian = false,
            BizhawkIdentifier = "GBC",
            MemoryLayout = new PlatformMemoryLayoutEntry[]
            {
                new PlatformMemoryLayoutEntry {
                    BizhawkIdentifier = "WRAM",
                    CustomPacketTransmitPosition = 0,
                    PhysicalStartingAddress = 0xC000,
                    Length = 0x2000
                },
                new PlatformMemoryLayoutEntry {
                    BizhawkIdentifier = "VRAM",
                    CustomPacketTransmitPosition = 0x2000 + 1,
                    PhysicalStartingAddress = 0x8000,
                    Length = 0x1FFF
                },
                new PlatformMemoryLayoutEntry {
                    BizhawkIdentifier = "HRAM",
                    CustomPacketTransmitPosition = 0x2000 + 0x1FFF + 1,
                    PhysicalStartingAddress = 0xFF80,
                    Length = 0x7E
                }
            }
        },
        new PlatformEntry
        {
            IsBigEndian = true,
            BizhawkIdentifier = "GBA",
            MemoryLayout = new PlatformMemoryLayoutEntry[]
            {
                new PlatformMemoryLayoutEntry
                {
                    BizhawkIdentifier = "EWRAM",
                    CustomPacketTransmitPosition = 0,
                    PhysicalStartingAddress = 0x02000000,
                    Length = 0x00040000
                },
                new PlatformMemoryLayoutEntry
                {
                    BizhawkIdentifier = "IWRAM",
                    CustomPacketTransmitPosition = 0x00040000 + 1,
                    PhysicalStartingAddress = 0x03000000,
                    Length = 0x00008000
                }
            }
        },
        new PlatformEntry()
        {
            IsBigEndian = true,
            BizhawkIdentifier = "NDS",
            FrameSkipDefault = 15,
            MemoryLayout = new PlatformMemoryLayoutEntry[] {
                new PlatformMemoryLayoutEntry {
                    BizhawkIdentifier = "Main RAM",
                    CustomPacketTransmitPosition = 0,
                    PhysicalStartingAddress = 0x2000000,
                    Length = 0x400000
                }
            }
        }
    };
}
#endregion