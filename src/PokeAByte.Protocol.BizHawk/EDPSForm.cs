using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Emulation.Common;
using PokeAByte.Protocol.BizHawk.PlatformData;

namespace PokeAByte.Protocol.BizHawk;

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

    public bool IsActive => true;
    public bool IsLoaded => true;
    private EmulatorProtocolServer? _server;
    private GameDataProcessor? _processor;

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

        Closing += (sender, args) => Cleanup();
    }

    private void StartServer()
    {
        _server = new EmulatorProtocolServer();
        _server.OnWrite = WriteToMemory;
        _server.OnSetup = Setup;
        _server.Start();
    }

    private void Cleanup()
    {
        MainLabel.Text = $"Waiting for Client...";
        _server?.Dispose();
        _server = null;
        _processor?.Dispose();
        _processor = null;
    }

    private void Setup(SetupInstruction instruction)
    {
        var system = APIs?.Emulation.GetGameInfo()?.System ?? string.Empty;
        var platform = PlatformConstants.Platforms.SingleOrDefault(x => x.SystemId == system);
        if (platform == null) {
            MainLabel.Text = $"Waiting for game to load";
            return;
        }
        if (_server == null) {
            MainLabel.Text = $"Failed to initialize properly.";
            return;
        }
        this._processor = new GameDataProcessor(
            MemoryDomains,
            platform,
            instruction,
            MainLabel
        );
    }

    private void WriteToMemory(WriteInstruction instruction)
    {
        if (instruction.Data.Length != 0 && APIs != null)
        {
            try
            {
                APIs.Memory.WriteByteRange(instruction.Address, instruction.Data);
            }
            catch (Exception) { }
        }
    }

    public void Restart()
    {
        Cleanup();
        StartServer();
        if (APIs?.Emulation.GetGameInfo() == null)
        {
            MainLabel.Text = "No game is loaded, doing nothing.";
        }
        else
        {
            MainLabel.Text = $"Waiting for client...";
        }
    }

    public bool AskSaveChanges() => true;

    public void UpdateValues(ToolFormUpdateType type)
    {
        if (type == ToolFormUpdateType.PostFrame)
        {
            this._processor?.Update();
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
