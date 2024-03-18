using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Common;
using BizHawk.Emulation.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GameHookIntegration;

[ExternalTool("GameHook Integration")]
public sealed class GameHookIntegrationForm : ToolFormBase, IExternalToolForm, IDisposable
{
    public ApiContainer? APIs { get; set; }

    [RequiredService]
    public IMemoryDomains? MemoryDomains { get; set; }

    protected override string WindowTitleStatic => "GameHook Integration";

    private readonly Label MainLabel = new() { Text = "Loading...", Height = 50, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top };

    private readonly MemoryMappedFile GameHookMetadata_MemoryMappedFile;
    private readonly MemoryMappedViewAccessor GameHookMetadata_Accessor;

    private readonly MemoryMappedFile? GameHookData_MemoryMappedFile;
    private readonly MemoryMappedViewAccessor? GameHookData_Accessor;

    private byte[] DataBuffer { get; } = new byte[SharedPlatformConstants.BIZHAWK_DATA_PACKET_SIZE];

    private string System = string.Empty;

    private SharedPlatformConstants.PlatformEntry? Platform = null;
    private int? FrameSkip = null;

    public GameHookIntegrationForm()
    {
        ShowInTaskbar = false;

        ClientSize = new(300, 60);
        SuspendLayout();

        Controls.Add(MainLabel);

        ResumeLayout(performLayout: false);
        PerformLayout();

        GameHookMetadata_MemoryMappedFile = MemoryMappedFile.CreateOrOpen("GAMEHOOK_BIZHAWK.bin", SharedPlatformConstants.BIZHAWK_METADATA_PACKET_SIZE, MemoryMappedFileAccess.ReadWrite);
        GameHookMetadata_Accessor = GameHookMetadata_MemoryMappedFile.CreateViewAccessor();

        GameHookData_MemoryMappedFile = MemoryMappedFile.CreateOrOpen("GAMEHOOK_BIZHAWK_DATA.bin", SharedPlatformConstants.BIZHAWK_DATA_PACKET_SIZE, MemoryMappedFileAccess.ReadWrite);
        GameHookData_Accessor = GameHookData_MemoryMappedFile.CreateViewAccessor();
    }

    public override void Restart()
    {
        var data = new byte[SharedPlatformConstants.BIZHAWK_METADATA_PACKET_SIZE];

        data[0] = 0x00;
        data[1] = SharedPlatformConstants.BIZHAWK_INTEGRATION_VERSION;

        System = APIs?.Emulation.GetGameInfo()?.System ?? string.Empty;
        Array.Copy(Encoding.UTF8.GetBytes(System), 0, data, 2, System.Length);

        GameHookMetadata_Accessor.WriteArray(0, data, 0, data.Length);

        Platform = SharedPlatformConstants.Information.SingleOrDefault(x => x.BizhawkIdentifier == System);

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

            MainLabel.Text = $"Sending {System} data to GameHook...";
        }
    }

    protected override void UpdateAfter()
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

                    GameHookData_Accessor?.WriteArray(entry.CustomPacketTransmitPosition, DataBuffer, 0, entry.Length);
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