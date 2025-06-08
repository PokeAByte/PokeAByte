using System.IO.MemoryMappedFiles;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Protocol;

namespace PokeAByte.Infrastructure.Drivers.PokeAProtocol;

public class PokeAProtocolClient : IDisposable
{
    private IPEndPoint _endpoint;
    private UdpClient _client;
    private CancellationTokenSource _connectionCts;
    private int _fileSize;

    public PokeAProtocolClient()
    {
        _endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55356);
        _client = new UdpClient();
        _client.Client.SetSocketOption(
            SocketOptionLevel.Socket,
            SocketOptionName.ReuseAddress,
            true
        );
        _connectionCts = new();
        _client.Connect(_endpoint);
    }

    public void WriteToBizhawk(WriteInstruction instruction, int timeoutMs = 100)
    {
        try
        {
            _client.Send(instruction.GetByteArray());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private MemoryMappedViewAccessor? GetMemoryAccessor()
    {
        using MemoryMappedFile mmfData = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? MemoryMappedFile.OpenExisting(
                SharedConstants.MemoryMappedFileName,
                MemoryMappedFileRights.Read
            )
            : MemoryMappedFile.CreateFromFile(
                $"/dev/shm/{SharedConstants.MemoryMappedFileName}",
                FileMode.Open,
                null,
                _fileSize,
                MemoryMappedFileAccess.Read
            );
        return mmfData.CreateViewAccessor(0, _fileSize, MemoryMappedFileAccess.Read);
    }
    
    public async ValueTask Setup(ReadBlock[] blocks, int fileSize, int frameSkip, int timeoutMs = 64)
    {
        try
        {
            var instruction = new SetupInstruction(blocks, frameSkip);
            _fileSize = fileSize;
            await _client.SendAsync(instruction.GetByteArray());
            using var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(timeoutMs);
            var response = await _client.ReceiveAsync(tokenSource.Token);
            if (response.Buffer[4] == Instructions.SETUP)
            {
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
        using var memoryAccessor = GetMemoryAccessor(); ;
        if (memoryAccessor == null)
        {
            throw new VisibleException("Poke-A-Protocol communication timed out. Memory mapped file was null.");
        }
        try
        {
            memoryAccessor.SafeMemoryMappedViewHandle.ReadSpan((ulong)position, block.Data.AsSpan());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void Dispose()
    {
        _connectionCts.Cancel();
        _connectionCts.Dispose();
        _client.Dispose();
    }
}