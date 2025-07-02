using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PokeAByte.Protocol;

public delegate void WriteHandler(WriteInstruction instruction);
public delegate void SetupHandler(SetupInstruction instruction);

public class EmulatorProtocolServer : IDisposable
{
    private const int PORT = 55356;
    private byte[] _buffer = new byte[65_535];
    private object _state = new();
    private bool _disposed;
    private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private EndPoint _endpoint = new IPEndPoint(IPAddress.Any, 0);
    private Thread? _thread;
    private List<int> _clientPorts = [];

    public WriteHandler? OnWrite { get; set; }
    public SetupHandler? OnSetup { get; set; }

    public EmulatorProtocolServer()
    {
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        _socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT));
    }

    private void Receive()
    {
        if (_disposed)
        {
            return;
        }
        _socket.BeginReceiveFrom(_buffer, 0, _buffer.Length, SocketFlags.None, ref _endpoint, OnData, _state);
    }

    private void OnData(IAsyncResult ar)
    {
        if (_disposed)
        {
            return;
        }
        int length = _socket.EndReceiveFrom(ar, ref _endpoint);
        var message = new byte[length];
        Buffer.BlockCopy(_buffer, 0, message, 0, length);
        var clientPort =(_endpoint as IPEndPoint)?.Port ?? 0;
        if (clientPort != 0 && !_clientPorts.Contains(clientPort)) {
            _clientPorts.Add(clientPort);
        }
        HandleMessage(_buffer);
        Receive();
    }

    private void HandleMessage(byte[] message)
    {
        if (_disposed)
        {
            return;
        }
        var protocolVersion = message[0];
        var instructionType = message[4];
        switch (instructionType)
        {
            case Instructions.PING:
                try
                {
                    _socket.SendTo(new PingResponse().GetByteArray(), _endpoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                break;
            case Instructions.NOOP:
                break;
            case Instructions.WRITE:
                WriteInstruction instruction = WriteInstruction.FromByteArray(message);
                OnWrite?.Invoke(instruction);
                break;
            case Instructions.SETUP:
                try
                {
                    OnSetup?.Invoke(SetupInstruction.FromByteArray(message));
                    _socket.SendTo(new SetupResponse().GetByteArray(), _endpoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                break;
            default:
                return;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_socket != null)
            {
                var close = new CloseInstruction().GetByteArray();
                foreach (var clientPort in _clientPorts)
                {
                    _socket.SendTo(close, new IPEndPoint(IPAddress.Parse("127.0.0.1"), clientPort));
                }
            }
            _disposed = true;
            _thread?.Abort();
            _socket?.Dispose();
        }
    }

    public void Start()
    {
        _thread = new Thread(Receive);
        _thread.Start();
    }
}
