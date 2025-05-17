using System.Net;
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
                    _udpClientWrapper.Dispose();
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
        bool result = await _udpClientWrapper.SendReadCommandAsync(transferBlock);
        if (!result)
        {
            Logger.LogDebug($"A timeout occurred when waiting for ReadMemoryAddress reply from RetroArch. (READ_CORE_MEMORY)");
            throw new DriverTimeoutException(transferBlock.Start, ProperName, null);
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