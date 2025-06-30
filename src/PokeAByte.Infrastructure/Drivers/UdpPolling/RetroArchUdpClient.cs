using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
    public bool IsClientConnected => _client is not null;

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

    private static string ToHexadecimal(uint value) => $"{value:X}".ToLower();

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
        for (int i = 0; i < value.Length; i++)
        {
            // While we do technically get ASCII and byte.Parse(ROS<byte>) parses utf8, we can still use it because
            // UTF8 is backwards compatible with ASCII:
            value[i] = ParseHexByte(input.Slice(offset, 2));
            offset += (i == value.Length - 1) ? 2 : 3; // Last byte has no trailing space
        }
        return value;
    }


    public async Task<bool> ReceiveAsync(CancellationToken cancellationToken)
    {
        if (!IsClientConnected)
        {
            Console.WriteLine("not connected to client");
            return false;
        }
        try
        {
            var response = await _client.ReceiveAsync(cancellationToken);
            Span<byte> bytes = response.Buffer;

            // Trim any trailing whitespace (including newlines)
            while (bytes.Length > 0 && (bytes[^1] == '\n' || bytes[^1] == '\r' || bytes[^1] == ' '))
            {
                bytes = bytes[..^1];
            }

            if (bytes.StartsWith(_readResponseStart))
            {
                // Skip "READ_CORE_MEMORY " (note the space)
                bytes = bytes.Slice(_readResponseStart.Length + 1);
                int space = bytes.IndexOf((byte)' ');
                if (space > 0)
                {
                    uint address = uint.Parse(bytes[..space], NumberStyles.HexNumber);
                    var dataSpan = bytes.Slice(space + 1);
                    var data = ParseReadMemoryResponse(dataSpan);
                    _responses[address] = data;
                }
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
        return _client.SendAsync(Encoding.ASCII.GetBytes($"WRITE_CORE_MEMORY {arguments}"));
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
    public async ValueTask<byte[]?> SendReadCommandAsync(uint start, uint length)
    {
        if (!IsClientConnected)
        {
            throw new Exception(
                $"Connection to emulator UDP server lost. Unable to send command \"READ_CORE_MEMORY {start} {length}\""
            );
        }
        byte[]? response = null;
        _ = await _client.SendAsync(
            Encoding.UTF8.GetBytes(string.Concat("READ_CORE_MEMORY ", ToHexadecimal(start), " ", length.ToString()))
        );
        SpinWait.SpinUntil(() =>
            {
                return _responses.TryGetValue(start, out response);
            },
            TimeSpan.FromMilliseconds(_timeout)
        );
        return response;
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