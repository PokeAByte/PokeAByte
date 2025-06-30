using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;

namespace PokeAByte.Infrastructure.Drivers.UdpPolling;

/// <summary>
/// Driver to communicate with RetroArch and compatible emulators.
/// </summary>
public class RetroArchUdpDriver : IPokeAByteDriver, IRetroArchUdpPollingDriver
{
    private static uint GetLoopbackMtu()		
    {
        var loopbackDevice = NetworkInterface.GetAllNetworkInterfaces()		
            .FirstOrDefault(x => x.NetworkInterfaceType == NetworkInterfaceType.Loopback);
		
        if (loopbackDevice == null)		
        {		
            // fallback to what seems to be the macOS default, which is smaller than on all other platforms.		
            return 16384u;		
        }		
        var result = (uint)loopbackDevice.GetIPProperties().GetIPv4Properties().Mtu;		
        return result;		
    }

    // MTU minus some overhead, divided by 3 because we get 3 characters per byte requested:		
    private static uint _maxChunkSize = (uint)Math.Floor((GetLoopbackMtu() - 128) / 3d);

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

    private static string ToRetroArchHexdecimalString(uint value) => value <= 9 ? value.ToString() : $"{value:x2}";

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
                    break;
                }
            }
            _udpClientWrapper.Dispose();
            _udpClientWrapper = null;
        });
        return Task.CompletedTask;
    }

    private async ValueTask<bool> ReadMemoryAddress(BlockData transferBlock)
    {
        if (_udpClientWrapper == null)
        {
            Logger.LogDebug("Can not read memory, UDP client is unitialized.");
            throw new DriverTimeoutException(transferBlock.Start, ProperName, null);
        }
        uint length = (uint)transferBlock.Data.Length;
        if (length <= _maxChunkSize)
        {
            byte[]? response = await _udpClientWrapper.SendReadCommandAsync(transferBlock.Start, length);
            if (response == null)
            {
                Logger.LogDebug($"(unchunked) A timeout occurred when waiting for ReadMemoryAddress reply from RetroArch. (READ_CORE_MEMORY)");
                throw new DriverTimeoutException(transferBlock.Start, ProperName, null);
            }
            response.AsSpan().CopyTo(transferBlock.Data);
        }
        else
        {
            // Large read - break into chunks		
            uint offset = 0;
            while (offset < length)
            {
                uint chunkSize = Math.Min(_maxChunkSize, length - offset);
                uint currentAddress = transferBlock.Start + offset;
                var command = $"READ_CORE_MEMORY";
                byte[]? chunk = await _udpClientWrapper.SendReadCommandAsync(currentAddress, chunkSize);
                if (chunk == null)
                {
                    Logger.LogDebug($"(chunked) A timeout occurred when waiting for ReadMemoryAddress reply from RetroArch. ({command})");
                    throw new DriverTimeoutException(currentAddress, ProperName, null);
                }
                Array.Copy(chunk, 0, transferBlock.Data, offset, chunk.Length);
                offset += (uint)chunk.Length;
            }
        }
        return true;
    }

    /// <inheritdoc/>
    public async Task EstablishConnection()
    {
        _connectionCts = new CancellationTokenSource();
        await ConnectAsync(_connectionCts.Token);
    }

    /// <inheritdoc/>
    public async Task Disconnect()
    {
        if (_connectionCts != null)
        {
            await _connectionCts.CancelAsync();
            _connectionCts.Dispose();
            _connectionCts = null;
        }
    }

    /// <inheritdoc/>
    public async ValueTask WriteBytes(uint memoryAddress, byte[] values, string? path = null)
    {
        if (_udpClientWrapper == null)
        {
            return;
        }
        var bytes = string.Join(' ', values.Select(x => x.ToHexdecimalString()));
        await _udpClientWrapper.SendWriteCommand($"{ToRetroArchHexdecimalString(memoryAddress)} {bytes}");
    }

    /// <inheritdoc/>
    public async ValueTask ReadBytes(BlockData[] transferBlocks)
    {
        foreach (BlockData block in transferBlocks)
        {
            await ReadMemoryAddress(block);
        }
    }

    /// <inheritdoc/>
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
        try
        {
            client.Connect(endpoint);
            await client.SendAsync("READ_CORE_MEMORY 0 1"u8.ToArray());
            var result = await client.ReceiveAsync();
            return result.Buffer.AsSpan().StartsWith("READ_CORE_MEMORY 0"u8);
        }
        catch (Exception)
        {
            return false;
        }
    }
}