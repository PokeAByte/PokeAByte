using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;

namespace PokeAByte.Infrastructure.Drivers.UdpPolling;

public class RetroArchUdpDriver : IPokeAByteDriver, IRetroArchUdpPollingDriver
{
    private CancellationTokenSource? _connectionCts;
    public string ProperName { get; } = "RetroArch";
    public int DelayMsBetweenReads { get; }
    private ILogger<RetroArchUdpDriver> Logger { get; }
    private readonly AppSettings _appSettings;
    private RetroArchUdpClient? _udpClientWrapper;

    public RetroArchUdpDriver(ILogger<RetroArchUdpDriver> logger, AppSettings appSettings)
    {
        Logger = logger;
        DelayMsBetweenReads = appSettings.RETROARCH_DELAY_MS_BETWEEN_READS;
        _appSettings = appSettings;
        
    }

    private static string ToRetroArchHexdecimalString(uint value) => value <= 9 ? $"{value}" : $"{value:X2}".ToLower();

    private Task ConnectAsync(CancellationToken cancellationToken)
    {
        _udpClientWrapper = new RetroArchUdpClient(
            _appSettings.RETROARCH_LISTEN_IP_ADDRESS,
            _appSettings.RETROARCH_LISTEN_PORT,
            _appSettings.RETROARCH_READ_PACKET_TIMEOUT_MS
        );
        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!await _udpClientWrapper.ReceiveAsync(cancellationToken))
                {
                    _udpClientWrapper.Dispose();
                }
            }
            _udpClientWrapper.Dispose();
            _udpClientWrapper = null;
        });
        return Task.CompletedTask;
    }

    private async Task<byte[]> ReadMemoryAddress(uint memoryAddress, uint length)
    {
        if (_udpClientWrapper == null) 
        {
            Logger.LogDebug("Can not read memory, UDP client is unitialized.");
            throw new DriverTimeoutException(memoryAddress, ProperName, null);
        }
        var command = $"READ_CORE_MEMORY";
        byte[]? response = await _udpClientWrapper.SendCommandAsync(
            command, 
            ToRetroArchHexdecimalString(memoryAddress), 
            length.ToString()
        );
        if (response == null)
        {
            Logger.LogDebug($"A timeout occurred when waiting for ReadMemoryAddress reply from RetroArch. ({command})");
            throw new DriverTimeoutException(memoryAddress, ProperName, null);
        }
        return response;
    }

    /// <summary>
    /// A no-op interface implementation.
    /// </summary>
    /// <returns> A completed task. </returns>
    public async Task EstablishConnection() {
        _connectionCts = new CancellationTokenSource();
        await ConnectAsync(_connectionCts.Token);
    }
    

    public async Task Disconnect() {
        if (_connectionCts != null)
        {
            await _connectionCts.CancelAsync();
            _connectionCts.Dispose();
            _connectionCts = null;
        }
    }

    /// <summary>
    /// Tell retroarch to write target bytes into the game memory.
    /// </summary>
    /// <param name="memoryAddress"> The address as which to start writing. </param>
    /// <param name="values"> The bytes to write to memory. </param>
    /// <returns> An awaitable task. </returns>
    public async Task WriteBytes(uint memoryAddress, byte[] values, string? path = null)
    {
        if (_udpClientWrapper == null) {
            return;
        }
        var bytes = string.Join(' ', values.Select(x => x.ToHexdecimalString()));
        await _udpClientWrapper.SendAsync(
            "WRITE_CORE_MEMORY",
            $"{ToRetroArchHexdecimalString(memoryAddress)} {bytes}"
        );
    }

    /// <summary>
    /// Read target memory blocks from retroarch.
    /// </summary>
    /// <param name="blocks"> Which blocks to read. </param>
    /// <returns> 
    /// The dictionary of read blocks. <br/>
    /// Key is the start of each address block. <br/>
    /// Value is the byte array contained in that block.
    /// </returns>
    public async Task<BlockData[]> ReadBytes(IList<MemoryAddressBlock> blocks)
    {
        var result = new BlockData[blocks.Count];
        var tasks =  blocks.Select(async (block) => {
            var data = await ReadMemoryAddress(block.StartingAddress, block.EndingAddress - block.StartingAddress + 1);
            return new BlockData(block.StartingAddress, data);
        });
        return await Task.WhenAll(tasks);
    }

    public static async Task<bool> Probe(AppSettings appSettings)
    {
        using var client = new UdpClient();
        IPEndPoint endpoint = new IPEndPoint(
            IPAddress.Parse(appSettings.RETROARCH_LISTEN_IP_ADDRESS), 
            appSettings.RETROARCH_LISTEN_PORT
        );
        client.Client.SetSocketOption(
            SocketOptionLevel.Socket,
            SocketOptionName.ReuseAddress,
            true
        );
        try {
            client.Connect(endpoint);
            await client.SendAsync("READ_CORE_MEMORY 0 1"u8.ToArray());
            var result = await client.ReceiveAsync();
            return result.Buffer.AsSpan().StartsWith("READ_CORE_MEMORY 0"u8);
        } catch (Exception) {
            return false;
        }
    }
}