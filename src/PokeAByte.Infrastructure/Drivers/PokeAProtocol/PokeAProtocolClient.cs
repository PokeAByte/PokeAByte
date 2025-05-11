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
    private MemoryMappedViewAccessor? _memoryAccessor;

    public PokeAProtocolClient() {
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

    public async ValueTask Setup(ReadBlock[] blocks, int fileSize, int timeoutMs = 64)
    {
        try
        {
            var instruction = new SetupInstruction(blocks);
            _client.Send(instruction.GetByteArray());
            var response = await _client.ReceiveAsync();
            if (response.Buffer[4] == Instructions.SETUP) 
            {
                using MemoryMappedFile mmfData = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? MemoryMappedFile.OpenExisting("EDPS_MemoryData.bin", MemoryMappedFileRights.Read)
                    : MemoryMappedFile.CreateFromFile("/dev/shm/EDPS_MemoryData.bin", FileMode.Open, null, fileSize, MemoryMappedFileAccess.Read);
                _memoryAccessor = mmfData.CreateViewAccessor(0, fileSize, MemoryMappedFileAccess.Read);
                return;
            }
        }
        catch (Exception)
        {
            throw new VisibleException("Poke-A-Protocol communication timed out");
            throw;
        }
    }

    public void Read(ulong position, BlockData block, int timeoutMs = 64)
    {
        if (_memoryAccessor == null) {
            throw new VisibleException("Poke-A-Protocol communication timed out");
        }
        try
        {
            _memoryAccessor.SafeMemoryMappedViewHandle.ReadSpan((ulong)position, block.Data.AsSpan());
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
        _memoryAccessor?.Dispose();
        _memoryAccessor = null;
        _client.Dispose();
    }
}