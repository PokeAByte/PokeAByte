using System.Net;
using System.Net.Sockets;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Protocol;

namespace PokeAByte.Infrastructure.Drivers.PokeAProtocol;

public class PokeAProtocolDriver : IPokeAByteDriver
{
    public string ProperName => "PokeAProtocol";
    public int DelayMsBetweenReads { get; }
    private PokeAProtocolClient? _client;
    private ReadBlock[]? _readBlocks;

    public PokeAProtocolDriver(AppSettings appSettings)
    {
        DelayMsBetweenReads = appSettings.BIZHAWK_DELAY_MS_BETWEEN_READS;
    }

    public Task EstablishConnection()
    {
        _client = new PokeAProtocolClient();
        return Task.CompletedTask;
    }

    public Task Disconnect() {
        _client?.Dispose();
        _client = null;
        return Task.CompletedTask;
    }

    public async ValueTask ReadBytes(BlockData[] blocks)
    {
        if (_client == null) {
            throw new Exception($"No connection to bizhawk");
        }
        if (_readBlocks == null) {
            _readBlocks = new ReadBlock[blocks.Length];
            int position = 0;
            int fileSize = 0;
            for (int i = 0; i < blocks.Length; i++) {
                _readBlocks[i].GameAddress = blocks[i].Start;
                _readBlocks[i].Length = blocks[i].Data.Length;
                _readBlocks[i].Position = (uint)position;
                if (i > 0) {
                    position += _readBlocks[i].Length;
                }
                fileSize += _readBlocks[i].Length;
            }
            try {
                if (fileSize == 0) {
                    throw new Exception("asdjad");
                }
                await _client.Setup(_readBlocks, fileSize);
            } catch (Exception ex) {
                Console.WriteLine(ex);
                return;
            }
        }
        for(int i = 0; i < blocks.Length; i++) {
            var position = _readBlocks.Where(x => x.GameAddress == blocks[i].Start).First().Position;
            _client.Read(position,blocks[i]);
        }
    }

    public ValueTask WriteBytes(uint startingMemoryAddress, byte[] values, string? path = null)
    {
        if (_client == null)
        {
            throw new Exception($"No connection to bizhawk.");
        }
        WriteInstruction instruction = new()
        {
            Address = startingMemoryAddress,
            Data = values,
            Length = values.Length,
        };
        _client.WriteToBizhawk(instruction);
        return ValueTask.CompletedTask;
    }

    public static async Task<bool> Probe(AppSettings appSettings)
    {
        using var client = new UdpClient();
        var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55356);
        client.Client.SetSocketOption(
            SocketOptionLevel.Socket,
            SocketOptionName.ReuseAddress,
            true
        );
        try
        {
            client.Connect(endpoint);
            using CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(32);

            await client.SendAsync(new PingInstruction().GetByteArray());
            var result = await client.ReceiveAsync(cancellationTokenSource.Token);
            return result.Buffer[4] == Instructions.PING && result.Buffer[5] == 0x01;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
