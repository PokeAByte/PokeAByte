using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Emulation.Common;
using PokeAByte.Protocol.BizHawk.PlatformData;

namespace PokeAByte.Protocol.BizHawk;

[ExternalTool("Emulator Data Protocol Server")]
public sealed class EDPSForm : Form, IExternalToolForm
{
    public ApiContainer? APIs { get; set; }

    [RequiredService]
    public IMemoryDomains? MemoryDomains { get; set; } = null;

    private readonly Label MainLabel = new()
    {
        Text = "Loading...",
        Height = 50,
        TextAlign = ContentAlignment.MiddleCenter,
        Dock = DockStyle.Top
    };

    public bool IsActive { get; private set; } = true;
    public bool IsLoaded => true;
    private EmulatorProtocolServer? _server;
    private string _initializedGame = "";
    private GameDataProcessor? _processor;

    public EDPSForm()
    {
        SuspendLayout();

        base.Text = "Emulator Data Protocol Server";
        ShowInTaskbar = false;

        ClientSize = new(300, 60);

        Closing += (_, _) =>
        {
            Cleanup();
            IsActive = false;
        };
        Controls.Add(MainLabel);

        ResumeLayout(performLayout: true);
    }

    private void StartServer()
    {
        _server = new EmulatorProtocolServer
        {
            OnWrite = (instruction) => _processor?.QueueWrite(instruction),
            OnSetup = Setup,
            OnFreeze = (instruction) => _processor?.AddFreeze(instruction),
            OnUnfreeze = (instruction) => _processor?.RemoveFreeze(instruction),
            OnCloseRequest = () =>
            {
                Cleanup();
                StartServer();
            }
        };
        _server.Start();
    }

    private void Cleanup()
    {
        _server?.Dispose();
        _server = null;
        _processor?.Dispose();
        _processor = null;
        MainLabel.Text = $"Waiting for Client...";
    }

    private void Setup(SetupInstruction instruction)
    {
        if (_processor != null)
        {
            _processor.Dispose();
            _processor = null;
        }
        var gameInfo = APIs?.Emulation.GetGameInfo();
        var system = gameInfo?.System ?? string.Empty;
        var platform = PlatformConstants.Platforms.SingleOrDefault(x => x.SystemId == system);
        if (platform == null || gameInfo == null)
        {
            MainLabel.Text = $"Waiting for game to load";
            return;
        }
        if (_server == null)
        {
            MainLabel.Text = $"Failed to initialize properly.";
            return;
        }
        this._initializedGame = gameInfo.Name + gameInfo.Hash;
        this._processor = new GameDataProcessor(
            platform,
            instruction,
            MainLabel
        );
    }

    public void Restart()
    {
        var gameInfo = APIs?.Emulation.GetGameInfo();
        var gameIdentifier = gameInfo != null
            ? gameInfo.Name + gameInfo.Hash
            : null;
        if (gameIdentifier != this._initializedGame)
        {
            Cleanup();
            StartServer();
            MainLabel.Text = gameInfo == null
                ? "No game is loaded, doing nothing."
                : $"Waiting for client...";
        }
    }

    public bool AskSaveChanges() => true;

    public void UpdateValues(ToolFormUpdateType type)
    {
        if (type == ToolFormUpdateType.PostFrame && this.MemoryDomains != null)
        {
            this._processor?.WriteFreezes(this.MemoryDomains);
            this._processor?.UpdateGameMemory(this.MemoryDomains);
        }
    }
}
