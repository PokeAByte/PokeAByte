using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PokeAByte.Infrastructure.Drivers.UdpPolling;

public class UdpClientWrapper : IDisposable
{
    private UdpClient? _client;
    private bool _isDisposed = false;
    private bool _isConnected = false;
    private readonly IPAddress _ipAddress;
    private readonly int _port;
    public bool IsConnected => _isConnected;
    public bool IsDisposed => _isDisposed;

    public bool IsClientConnected =>
        _isDisposed is false &&  
        _isConnected && 
        _client is not null &&
        _client.Client.Connected;
    public UdpClientWrapper(string host, int port)
    {
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
    public void Connect()
    {
        if(IsClientConnected) return;
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
        _client?.Connect(_ipAddress, _port);
        _isConnected = true;
    }

    public async Task SendPacketAsync(string command, string argument)
    {
        var outgoingPayload = $"{command} {argument}";
        var datagram = Encoding.ASCII.GetBytes(outgoingPayload);
        if (!IsClientConnected)
        {
            Connect();
        }
        if (!IsClientConnected)
        {
            Dispose();
            throw new Exception($"Unable to create UdpClient to SendPacket({command} {argument})");
        }
        _ = await _client!.SendAsync(datagram, datagram.Length);
    }

    public async Task ReceivePacketAsync(Dictionary<string, ReceivedPacket> responses)
    {
        if (!IsClientConnected)
        {
            Connect();
        }
        if (!IsClientConnected)
        {
            Dispose();
            throw new Exception("UdpClient is still NULL when waiting for messages.");
        }
        var result = await _client!.ReceiveAsync();
        string receiveString = Encoding.ASCII.GetString(result.Buffer).Replace("\n", string.Empty);
        var splitString = receiveString.Split(' ');
        var command = splitString[0];
        var memoryAddressString = splitString[1];
        var valueStringArray = splitString[2..];

        if (valueStringArray[0] == "-1")
        {
            Dispose();
            throw new Exception(receiveString);
        }

        var memoryAddress = Convert.ToUInt32(memoryAddressString, 16);
        var value = valueStringArray.Select(x => Convert.ToByte(x, 16)).ToArray();

        var receiveKey = $"{command} {memoryAddressString} {valueStringArray.Length}";

        responses[receiveKey] = new ReceivedPacket(command, memoryAddress, value);
    }

    public void Dispose()
    {
        _isConnected = false;
        _isDisposed = true;
        _client?.Dispose();
        _client = null;
    }
}