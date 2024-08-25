using System.Globalization;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;

namespace PokeAByte.Infrastructure.Drivers.UdpPolling;

public class NewRetroArchUdpDriver : IPokeAByteDriver, IRetroArchUdpPollingDriver
{
    private CancellationTokenSource _connectionCts = new();

    public string ProperName { get; } = "NewRetroArch";
    public int DelayMsBetweenReads { get; }
    private ILogger<RetroArchUdpPollingDriver> Logger { get; }
    private readonly AppSettings _appSettings;
    private RetroArchUdpClient _udpClientWrapper;

    public NewRetroArchUdpDriver(ILogger<RetroArchUdpPollingDriver> logger, AppSettings appSettings)
    {
        Logger = logger;
        _appSettings = appSettings;
        _udpClientWrapper = new RetroArchUdpClient(
            _appSettings.RETROARCH_LISTEN_IP_ADDRESS,
            _appSettings.RETROARCH_LISTEN_PORT,
            _appSettings.RETROARCH_READ_PACKET_TIMEOUT_MS
        );

        DelayMsBetweenReads = appSettings.RETROARCH_DELAY_MS_BETWEEN_READS;
        Task.Run(async () =>
        {
            await ConnectAsync(_connectionCts.Token);
        });
    }

    private static string ToRetroArchHexdecimalString(uint value) => value <= 9 ? $"{value}" : $"{value:X2}".ToLower();

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _udpClientWrapper.Connect();
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _udpClientWrapper.ReceiveAsync();
            }
            catch
            {
                _udpClientWrapper.Dispose();
                // Automatically swallow exceptions here because
                // they're not useful even if there's an error.

                // We don't want to spam the user with errors.
            }
        }
    }

    private async Task<byte[]> ReadMemoryAddress(uint memoryAddress, uint length)
    {
        var command = $"READ_CORE_MEMORY {ToRetroArchHexdecimalString(memoryAddress)}";

        string? response = await _udpClientWrapper.SendReceiveAsync(command, $"{length}");
        if (response == null)
        {
            Logger.LogDebug($"A timeout occurred when waiting for ReadMemoryAddress reply from RetroArch. ({command})");
            throw new DriverTimeoutException(memoryAddress, "RetroArch", null);
        }

        var splitString = response.Split(' ');
        var valueStringArray = splitString[2..];

        if (valueStringArray[0] == "-1")
        {
            throw new Exception("Command: " + command + "\nReceived: " + response);
        }

        byte[] value = new byte[valueStringArray.Length];
        for (int i = 0; i < value.Length - 1; i++)
        {
            value[i] = byte.Parse(valueStringArray[i], NumberStyles.HexNumber);
        }
        return value;
    }

    /// <summary>
    /// A no-op interface implementation.
    /// </summary>
    /// <returns> A completed task. </returns>
    public Task EstablishConnection() => Task.CompletedTask;

    /// <summary>
    /// Tests the connect to retroarch by reading the very first byte of game memory.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the connection works. <see langword="false"/> if it does not.
    /// </returns>
    public async Task<bool> TestConnection()
    {
        try
        {
            MemoryAddressBlock testBlock = new("test", 0, 1);
            _ = await ReadBytes([testBlock]);
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "TestConnection");
            return false;
        }
    }

    /// <summary>
    /// Tell retroarch to write target bytes into the game memory.
    /// </summary>
    /// <param name="memoryAddress"> The address as which to start writing. </param>
    /// <param name="values"> The bytes to write to memory. </param>
    /// <returns> An awaitable task. </returns>
    public async Task WriteBytes(uint memoryAddress, byte[] values)
    {
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
    public async Task<Dictionary<uint, byte[]>> ReadBytes(IEnumerable<MemoryAddressBlock> blocks)
    {
        var results = await Task.WhenAll(blocks.Select(async x =>
        {
            // We add one here because otherwise the result is too short.

            // Example: 0xAFFF - 0xA000 is 4095 in decimal.
            // We want to actually return 4096 bytes - we want to include 0xAFFF.
            var data = await ReadMemoryAddress(x.StartingAddress, x.EndingAddress - x.StartingAddress + 1);
            return new KeyValuePair<uint, byte[]>(x.StartingAddress, data);
        }));

        return results.ToDictionary(x => x.Key, x => x.Value);
    }
}