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
    private bool _isDisposed = false;
    private bool _isConnected = false;
    private Dictionary<string, byte[]> _responses = [];
    private readonly int _timeout;
    private readonly IPAddress _ipAddress;
    private readonly int _port;

    [MemberNotNullWhen(true, nameof(_client))]
    public bool IsClientConnected =>
        _isDisposed is false &&
        _isConnected &&
        _client is not null &&
        _client.Client.Connected;

    public UdpReceiveResult? _receivedData { get; private set; }

    public RetroArchUdpClient(string host, int port, int timeout)
    {
        _timeout = timeout;
        _ipAddress = IPAddress.Parse(host);
        _port = port;
        CreateClient();
    }

    private void CreateClient()
    {
        _client = new UdpClient();
        _client.Client.SetSocketOption(SocketOptionLevel.Socket,
            SocketOptionName.ReuseAddress,
            true);
        _isDisposed = false;
    }

    [MemberNotNullWhen(true, nameof(_client))]
    private bool IsClientAlive()
    {
        if (!IsClientConnected)
        {
            Connect();
        }
        if (!IsClientConnected)
        {
            Dispose();
            return false;
        }
        return true;
    }

    public void Connect()
    {
        if (IsClientConnected)
        {
            return;
        }
        if (_isDisposed || _client is null)
        {
            _isConnected = false;
            CreateClient();
        }

        if (_isDisposed || _client is null)
        {
            Dispose();
            throw new Exception("UdpClient is still NULL when connecting.");
        }
        _client.Connect(_ipAddress, _port);
        _isConnected = true;
    }

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


    public async Task<bool> ReceiveAsync(CancellationToken cancellationToken)
    {
        if (!IsClientAlive())
        {
            return false;
        }
        try
        {
            var response = await _client.ReceiveAsync(cancellationToken);
            Span<byte> bytes = response.Buffer;
            if (bytes.StartsWith(_readResponseStart))
            {
                bytes = bytes.Slice(17);
                int space = bytes.IndexOf((byte)' ');
                string address = Encoding.ASCII.GetString(bytes[..space]);
                var data = ParseReadMemoryResponse(bytes.Slice(space + 1));
                _responses[string.Concat(address, "-", data.Length.ToString())] = data;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Send a command to the emulator without waiting for a response.
    /// </summary>
    /// <param name="command"> The emulator command. </param>
    /// <param name="argument"> Emulator command arguments. </param>
    /// <exception cref="Exception">
    /// The UDP client is unitiliazed, disconnected, has been disposed, or is in an otherwise unusable state.
    /// </exception>
    public async Task SendAsync(string command, string argument)
    {
        if (!IsClientAlive())
        {
            throw new Exception($"Unable to create UdpClient to SendPacket({command} {argument})");
        }
        _ = await _client.SendAsync(Encoding.ASCII.GetBytes($"{command} {argument}"));
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
    public async Task<byte[]?> SendCommandAsync(string command, string argument1, string argument2)
    {
        if (!IsClientAlive())
        {
            throw new Exception($"Unable to create UdpClient to SendPacket({command} {argument1} {argument2})");
        }
        byte[]? response = null;
        _ = await _client.SendAsync(Encoding.ASCII.GetBytes($"{command} {argument1} {argument2}"));
        string key = string.Concat(argument1, "-", argument2);
        SpinWait.SpinUntil(() =>
            {
                return _responses.TryGetValue(key, out response);
            },
            TimeSpan.FromMilliseconds(_timeout)
        );
        return response;
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _client?.Dispose();
        }
        _isConnected = false;
        _isDisposed = true;
        _client = null;
    }
}