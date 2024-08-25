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
            throw new Exception("UdpClient is still NULL when waiting for messages.");
        }
        this._receivedData = await _client.ReceiveAsync();
        return true;
    }

    public UdpReceiveResult? WaitForData()
    {
        UdpReceiveResult? response = null;
        // TODO: SpinWait is not ideal for this scenario.
        SpinWait.SpinUntil(() =>
        {
            response = _receivedData;
            return _receivedData != null;
        }, TimeSpan.FromMilliseconds(_timeout));
        _receivedData = null;
        return response;
    }

    public async Task SendAsync(string command, string argument)
    {
        if (!IsClientAlive())
        {
            throw new Exception($"Unable to create UdpClient to SendPacket({command} {argument})");
        }
        var outgoingPayload = $"{command} {argument}";
        var datagram = Encoding.ASCII.GetBytes(outgoingPayload);
        await semaphoreSlim.WaitAsync();
        _ = await _client.SendAsync(datagram, datagram.Length);
        _ = WaitForData();
        semaphoreSlim.Release();
    }

    public async Task<string?> SendReceiveAsync(string command, string argument)
    {
        if (!IsClientAlive())
        {
            throw new Exception($"Unable to create UdpClient to SendPacket({command} {argument})");
        }
        var outgoingPayload = $"{command} {argument}";
        var datagram = Encoding.ASCII.GetBytes(outgoingPayload);
        await semaphoreSlim.WaitAsync();
        _ = await _client.SendAsync(datagram, datagram.Length);
        UdpReceiveResult? udpData = WaitForData();
        semaphoreSlim.Release();
        if (udpData == null)
        {
            return null;
        }
        return Encoding.ASCII.GetString(udpData.Value.Buffer).Replace("\n", string.Empty);
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