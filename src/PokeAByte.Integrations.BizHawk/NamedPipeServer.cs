using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;

namespace PokeAByte.Integrations.BizHawk;

public delegate void ClientDataHandler(MemoryContract? memoryContract);

public class NamedPipeServer : IDisposable
{
    public event ClientDataHandler? ClientDataHandler;
    private string _pipeName = "";
    private NamedPipeServerStream? _pipeServer = null;
    public void StartServer(string pipeName)
    {
        _pipeName = pipeName;
        _pipeServer = new(pipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        Console.WriteLine("Pipe server created, waiting for connections...");
        _pipeServer.BeginWaitForConnection(
            WaitForConnectionCallback,
            _pipeServer);
    }

    private void WaitForConnectionCallback(IAsyncResult iar)
    {
        Console.WriteLine("Client connected");
        if (iar.AsyncState is null || _pipeServer is null)
            throw new InvalidOperationException(
                "The pipe server is null.");
        try
        {
            //var pipeServer = (NamedPipeServerStream)iar.AsyncState;
            _pipeServer.EndWaitForConnection(iar);
            Console.WriteLine("Reading 255 bytes of client data...");
            var buffer = new byte[255];
            var dataList = new List<byte>();
            var count = _pipeServer.Read(buffer, 0, 255);
            dataList.AddRange(buffer.Take(count));
            while (count == 255)
            {
                Console.WriteLine("Reading 255 more bytes of client data...");
                count = _pipeServer.Read(buffer, 0, 255);
                dataList.AddRange(buffer.Take(count));
            }
            Console.WriteLine($"Finished reading client data... Length: {dataList.Count}");
            ClientDataHandler?.Invoke(MemoryContract.Deserialize(dataList.ToArray()));
            Console.WriteLine("Invoked delegate complete, closing server");
            _pipeServer.Close();
            _pipeServer = null;
            _pipeServer = new NamedPipeServerStream(
                _pipeName,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous
            );
            Console.WriteLine("Pipe server created, waiting for connections...");
            _pipeServer.BeginWaitForConnection(WaitForConnectionCallback, _pipeServer);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Dispose()
    {
        _pipeServer?.Dispose();
    }
}