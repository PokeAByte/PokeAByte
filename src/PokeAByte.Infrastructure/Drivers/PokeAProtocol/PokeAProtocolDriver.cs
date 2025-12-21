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

    private int _frameSkip;
    private PokeAProtocolClient? _client;
    private ReadBlock[]? _readBlocks;

    public PokeAProtocolDriver(AppSettings appSettings)
    {
        DelayMsBetweenReads = appSettings.DELAY_MS_BETWEEN_READS;
        _frameSkip = appSettings.PROTOCOL_FRAMESKIP;
    }

    public Task EstablishConnection()
    {
        _client = new PokeAProtocolClient();
        return Task.CompletedTask;
    }

    public async Task Disconnect()
    {
        if (_client != null)
        {
            await _client.RequestCloseAsync();
            _client.Dispose();
            _client = null;
        }
    }

    public async ValueTask ReadBytes(BlockData[] blocks)
    {
        if (_client == null)
        {
            throw new Exception($"EDPS Driver: No connection to bizhawk");
        }
        if (_readBlocks == null)
        {
            _readBlocks = new ReadBlock[blocks.Length];
            int position = 0;
            int fileSize = 0;
            for (int i = 0; i < blocks.Length; i++)
            {
                _readBlocks[i].GameAddress = blocks[i].Start;
                _readBlocks[i].Length = blocks[i].Data.Length;
                _readBlocks[i].Position = (uint)position;
                position += _readBlocks[i].Length;
                fileSize += _readBlocks[i].Length;
            }
            if (fileSize == 0)
            {
                throw new Exception("EDPS Driver: Invalid filesize for the MMF. Can not connect to EDPS.");
            }
            await _client.Setup(_readBlocks, fileSize, _frameSkip);
        }
        for (int i = 0; i < blocks.Length; i++)
        {
            var position = _readBlocks[i].Position;
            _client.Read(position, blocks[i]);
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
        var remote = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55356);
        try
        {
            using CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(32);

            await client.SendAsync(new PingInstruction().GetByteArray(), remote);
            var result = await client.ReceiveAsync(cancellationTokenSource.Token);
            return result.Buffer[4] == Instructions.PING && result.Buffer[5] == 0x01;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
