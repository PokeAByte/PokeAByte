using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Infrastructure.Drivers.UdpPolling;

/// <summary>
/// A wrapper around <see cref="UdpClient"/> for talking to retroarch (and other compatible emulators).
/// </summary>
public class RetroArchUdpClient : IDisposable
{
    private static byte[] _readResponseStart = Encoding.ASCII.GetBytes("READ_CORE_MEMORY");
    private UdpClient? _client;
    private Dictionary<uint, byte[]> _responses = [];
    private readonly int _timeout;

    [MemberNotNullWhen(true, nameof(_client))]
    public bool IsClientConnected => _client != null && _client.Client.Connected;

    public RetroArchUdpClient(string host, int port, int timeout)
    {
        _timeout = timeout;
        _client = new UdpClient();
        _client.Client.SetSocketOption(
            SocketOptionLevel.Socket,
            SocketOptionName.ReuseAddress,
            true
        );
        _client.Connect(new IPEndPoint(IPAddress.Parse(host), port));
    }

    private static string ToRetroArchHexdecimalString(uint value) => value <= 9 ? value.ToString() : $"{value:x2}";

    private static int GetDigitFromHex(byte input)
    {
        return input switch
        {
            >= (byte)'a' and <= (byte)'f' => (input - (byte)'a' + 10),
            >= (byte)'A' and <= (byte)'F' => (input - (byte)'A' + 10),
            _ => input - '0',
        };
    }

    private static byte ParseHexByte(ReadOnlySpan<byte> bytes)
    {
        return (byte)((GetDigitFromHex(bytes[0]) << 4) | GetDigitFromHex(bytes[1]));
    }

    private static byte[] ParseReadMemoryResponse(ReadOnlySpan<byte> input)
    {
        var byteCount = input.Count((byte)' ') + 1;
        // Then we can skip over the remaining span 3 characters at a time, slicing out each character-pair for the
        // byte.Parse().
        int offset = 0;
        byte[] value = new byte[byteCount];
        for (int i = 0; i < value.Length - 1; i++)
        {
            // While we do technically get ASCII and byte.Parse(ROS<byte>) parses utf8, we can still use it because
            // UTF8 is backwards compatible with ASCII:
            value[i] = ParseHexByte(input.Slice(offset, 2));
            offset += 3;
        }
        return value;
    }

    public async ValueTask<bool> ReceiveAsync(CancellationToken cancellationToken)
    {
        if (!IsClientConnected)
        {
            throw new Exception("Connection to emulator UDP server lost.");
        }
        try
        {
            var response = await _client.ReceiveAsync(cancellationToken);
            ReadOnlySpan<byte> bytes = response.Buffer;
            if (bytes.StartsWith(_readResponseStart))
            {
                bytes = bytes.Slice(17);
                int space = bytes.IndexOf((byte)' ');
                uint address = uint.Parse(bytes[..space], System.Globalization.NumberStyles.HexNumber);
                _responses[address] = ParseReadMemoryResponse(bytes.Slice(space + 1));
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Send a write command to the emulator without waiting for a response.
    /// </summary>
    /// <param name="arguments"> Emulator command arguments. </param>
    /// <exception cref="Exception">
    /// The UDP client is unitiliazed, disconnected, has been disposed, or is in an otherwise unusable state.
    /// </exception>
    public ValueTask<int> SendWriteCommand(string arguments)
    {
        if (!IsClientConnected)
        {
            throw new Exception(
                $"Connection to emulator UDP server lost. Unable to send command \"WRITE_CORE_MEMORY {arguments}\""
            );
        }
        return _client.SendAsync(Encoding.UTF8.GetBytes($"WRITE_CORE_MEMORY {arguments}"));
    }

    /// <summary>
    /// Send a command to the emulator and return the response as a string.
    /// </summary>
    /// <param name="command"> The emulator command. </param>
    /// <param name="argument"> Emulator command arguments. </param>
    /// <returns> The response from the emulator, as an ASCII byte array. </returns>
    /// <exception cref="Exception">
    /// The UDP client is unitiliazed, disconnected, has been disposed, or is in an otherwise unusable state.
    /// </exception>
    public async ValueTask<bool> SendReadCommandAsync(BlockData block)
    {
        if (!IsClientConnected)
        {
            throw new Exception(
                $"Connection to emulator UDP server lost. Unable to send command \"READ_CORE_MEMORY {block.Start} {block.Data.Length}\""
            );
        }
        bool success = false;
        _ = await _client.SendAsync(
            Encoding.UTF8.GetBytes($"READ_CORE_MEMORY {ToRetroArchHexdecimalString(block.Start)} {block.Data.Length}")
        );
        SpinWait.SpinUntil(() =>
            {
                if (_responses.TryGetValue(block.Start, out var response))
                {
                    response.AsSpan().CopyTo(block.Data.AsSpan());
                    success = true;
                }
                return success;
            },
            TimeSpan.FromMilliseconds(_timeout)
        );
        return success;
    }

    public void Dispose()
    {
        if (_client != null)
        {
            _client.Dispose();
            _client = null;
        }
    }
}