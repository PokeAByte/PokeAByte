using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PokeAByte.Protocol;

public delegate void WriteHandler(WriteInstruction instruction);
public delegate void SetupHandler(SetupInstruction instruction);
public delegate void CloseRequestHandler();

public class EmulatorProtocolServer : IDisposable
{
    private const int PORT = 55356;
    private bool _disposed;
    private Socket _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private Thread? _thread;
    private List<IPEndPoint> _clients = [];
    private UdpClient _listener;

    public WriteHandler? OnWrite { get; set; }
    public CloseRequestHandler? OnCloseRequest { get; set; }
    public SetupHandler? OnSetup { get; set; }

    public EmulatorProtocolServer()
    {
        _listener = new UdpClient(PORT);
    }

    private void Receive()
    {
        if (!_disposed)
        {
            _listener.BeginReceive(OnData, new {});
        }
    }

    private void OnData(IAsyncResult ar)
    {
        if (!_disposed)
        {
            IPEndPoint remote = new(0, 0);
            var message = _listener.EndReceive(ar, ref remote);
            HandleMessage(message, remote);
            Receive();
        }
    }

    private void HandleMessage(byte[] message, IPEndPoint endpoint)
    {
        var protocolVersion = message[0];
        var instructionType = message[4];
        if (_disposed || message[5] != 0)
        {
            return;
        }

        try
        {
            switch (instructionType)
            {
                case Instructions.PING:
                    _socket.SendTo(new PingResponse().GetByteArray(), endpoint);
                    break;
                case Instructions.CLOSE:
                    OnCloseRequest?.Invoke();
                    // todo: handle.
                    break;
                case Instructions.NOOP:
                    break;
                case Instructions.WRITE:
                    WriteInstruction instruction = WriteInstruction.FromByteArray(message);
                    OnWrite?.Invoke(instruction);
                    break;
                case Instructions.SETUP:
                    OnSetup?.Invoke(SetupInstruction.FromByteArray(message));
                    _socket.SendTo(new SetupResponse().GetByteArray(), endpoint);
                    _clients.Add(endpoint);
                    break;
                default:
                    return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        if (_listener != null)
        {
            var close = new CloseInstruction(toClient: true).GetByteArray();
            foreach (var remote in _clients)
            {
                _listener.Send(close, close.Length, remote);
            }
        }
        _disposed = true;
        _thread?.Abort();
        _listener?.Dispose();
    }

    public void Start()
    {
        _thread = new Thread(Receive);
        _thread.Start();
    }
}
