using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PokeAByte.Infrastructure.Drivers.UdpPolling;

/// <summary>
/// A wrapper around <see cref="UdpClient"/> for talking to retroarch (and other compatible emulators).
/// </summary>
public class RetroArchUdpClient : IDisposable
{
    private SemaphoreSlim semaphoreSlim = new(1, 1);
    private UdpClient? _client;
    private bool _isDisposed = false;
    private bool _isConnected = false;
    private readonly int _timeout;
    private readonly IPAddress _ipAddress;
    private readonly int _port;
    private static AutoResetEvent _dataReceivedEvent = new AutoResetEvent(false);

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

    public async Task<bool> ReceiveAsync()
    {
        if (!IsClientAlive())
        {
            return false;
        }
        try
        {
            this._receivedData = await _client.ReceiveAsync();
            _dataReceivedEvent.Set();
            return true;
        }
        catch
        {
            return false;
        }
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
    public async Task<byte[]?> SendCommandAsync(string command, string argument)
    {
        if (!IsClientAlive())
        {
            throw new Exception($"Unable to create UdpClient to SendPacket({command} {argument})");
        }
        await semaphoreSlim.WaitAsync();
        byte[]? response = null;
        int retries = 4;
        while (retries > 0 && response == null)
        {
            _dataReceivedEvent.Reset();
            _ = await _client.SendAsync(Encoding.ASCII.GetBytes($"{command} {argument}"));
            _dataReceivedEvent.WaitOne(_timeout / 4);
            response = _receivedData?.Buffer;
            _receivedData = null;
            retries--;
        }
        semaphoreSlim.Release();
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