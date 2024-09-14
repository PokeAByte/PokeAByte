using System.Globalization;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;

namespace PokeAByte.Infrastructure.Drivers.UdpPolling;

public class RetroArchUdpDriver : IPokeAByteDriver, IRetroArchUdpPollingDriver
{
    private CancellationTokenSource _connectionCts = new();
    public string ProperName { get; } = "RetroArch";
    public int DelayMsBetweenReads { get; }
    private ILogger<RetroArchUdpDriver> Logger { get; }
    private readonly AppSettings _appSettings;
    private RetroArchUdpClient _udpClientWrapper;

    public RetroArchUdpDriver(ILogger<RetroArchUdpDriver> logger, AppSettings appSettings)
    {
        Logger = logger;
        DelayMsBetweenReads = appSettings.RETROARCH_DELAY_MS_BETWEEN_READS;
        _appSettings = appSettings;
        _udpClientWrapper = new RetroArchUdpClient(
            _appSettings.RETROARCH_LISTEN_IP_ADDRESS,
            _appSettings.RETROARCH_LISTEN_PORT,
            _appSettings.RETROARCH_READ_PACKET_TIMEOUT_MS
        );
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
            if (!await _udpClientWrapper.ReceiveAsync())
            {
                _udpClientWrapper.Dispose();
            }
        }
    }

    private static byte[] ParseReadMemoryResponse(ReadOnlySpan<byte> input)
    {
        // The response from the emulator is "READ_CORE_MEMORY <address> <byte> <byte> <byte> ...."
        // We cut off the command with the 17 offset, then find the first space after the address:
        input = input.Slice(17);
        input = input.Slice(input.IndexOf((byte)' ') + 1);
        var byteCount = input.Count((byte)' ') + 1;
        // Then we can skip over the remaining span 3 characters at a time, slicing out each character-pair for the
        // byte.Parse().
        int offset = 0;
        byte[] value = new byte[byteCount];
        for (int i = 0; i < value.Length - 1; i++)
        {
            // While we do technically get ASCII and byte.Parse(ROS<byte>) parses utf8, we can still use it because
            // UTF8 is backwards compatible with ASCII:
            value[i] = byte.Parse(input.Slice(offset, 2), NumberStyles.HexNumber);
            offset += 3;
        }
        return value;
    }

    private async Task<byte[]> ReadMemoryAddress(uint memoryAddress, uint length)
    {
        var command = $"READ_CORE_MEMORY {ToRetroArchHexdecimalString(memoryAddress)}";
        byte[]? response = await _udpClientWrapper.SendCommandAsync(command, $"{length}");
        if (response == null)
        {
            Logger.LogDebug($"A timeout occurred when waiting for ReadMemoryAddress reply from RetroArch. ({command})");
            throw new DriverTimeoutException(memoryAddress, "RetroArch", null);
        }
        return ParseReadMemoryResponse(response);
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
            _ = await ReadMemoryAddress(0, 1);
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
    public async Task WriteBytes(uint memoryAddress, byte[] values, string? path = null)
    {
        var bytes = string.Join(' ', values.Select(x => x.ToHexdecimalString()));
        _ = await _udpClientWrapper.SendCommandAsync(
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
        for (int i = 0; i < blocks.Count; i++)
        {
            MemoryAddressBlock? block = blocks[i];
            var data = await ReadMemoryAddress(block.StartingAddress, block.EndingAddress - block.StartingAddress + 1);
            result[i] = new BlockData(block.StartingAddress, data);
        }
        return result;
    }
}