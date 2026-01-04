using System.IO.MemoryMappedFiles;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Protocol;
using PokeAByte.Infrastructure.SharedMemory;

namespace PokeAByte.Infrastructure.Drivers.PokeAProtocol;

public class PokeAProtocolClient : IDisposable
{
    private IPEndPoint _remoteEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55356);
    private UdpClient _client;
    private CancellationTokenSource _connectionCts;
    private int _fileSize;
    private bool _disposed;
    private ISharedMemory? _sharedMemory;
    private bool _connected = false;

    public PokeAProtocolClient()
    {
        _client = new UdpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
        _connectionCts = new();
    }

    private void WaitForClose()
    {
        _ = Task.Run(async () =>
        {
            while (!_connectionCts.IsCancellationRequested)
            {
                try
                {
                    var response = await _client!.ReceiveAsync(_connectionCts.Token);
                    if (response.Buffer.AsSpan().SequenceEqual(new CloseInstruction().GetByteArray().AsSpan()))
                    {
                        _connected = false;
                        return;
                    }
                }
                catch (TaskCanceledException)
                {
                    // nothing to
                }
            }
        }, _connectionCts.Token);
    }

    public ValueTask<int> SendToEmulatorAsync(IEmulatorInstruction instruction)
    {
        if (!_connected) {
            throw new VisibleException("Poke-A-Protocol server closed the connection.");
        }
        return _client.SendAsync(instruction.GetByteArray(), _remoteEndpoint);
    }

    private ISharedMemory GetMemoryAccessor()
    {
        if (_sharedMemory == null)
        {
            try
            {
                _sharedMemory = ISharedMemory.Get(_fileSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new VisibleException("Poke-A-Protocol server closed the connection.");
            }
        }
        
        return _sharedMemory;
    }

    public async ValueTask Setup(ReadBlock[] blocks, int fileSize, int frameSkip, int timeoutMs = 128)
    {
        try
        {
            var instruction = new SetupInstruction(blocks, frameSkip);
            _fileSize = fileSize;
            await _client!.SendAsync(instruction.GetByteArray(), _remoteEndpoint);
            using var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(timeoutMs);
            var response = await _client.ReceiveAsync(tokenSource.Token);
            if (response.Buffer[4] == Instructions.SETUP)
            {
                _connected = true;
                this.WaitForClose();
                return;
            }
        }
        catch (Exception ex)
        {
            throw new VisibleException($"Poke-A-Protocol communication timed out. Inner exception: {ex.Message}");
        }
    }

    public void Read(ulong position, BlockData block, int timeoutMs = 64)
    {
        var memoryAccessor = GetMemoryAccessor();
        if (!_connected) {
            throw new VisibleException("Poke-A-Protocol server closed the connection.");
        }
        
        memoryAccessor.CopyBytesToSpan(position, block.Data.Span);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _sharedMemory?.Dispose();
            _connectionCts.Cancel();
            _connectionCts.Dispose();
            _client.Dispose();
        }
    }

    internal async Task RequestCloseAsync()
    {
        var instruction = new CloseInstruction(toClient: false);
        await _client!.SendAsync(instruction.GetByteArray(), _remoteEndpoint);
    }
}