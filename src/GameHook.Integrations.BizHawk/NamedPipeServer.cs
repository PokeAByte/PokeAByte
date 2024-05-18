using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using GameHook.Contracts;

namespace GameHookIntegration;

public delegate void ClientDataHandler(MemoryContract<byte[]>? memoryContract);

public class NamedPipeServer
{
    public event ClientDataHandler? ClientDataHandler;
    private string _pipeName = "";
    public void StartServer(string pipeName)
    {
        _pipeName = pipeName;
        NamedPipeServerStream pipeServer = new(pipeName,
            PipeDirection.In,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        Console.WriteLine("Pipe server created, waiting for connections...");
        pipeServer.BeginWaitForConnection(
            WaitForConnectionCallback,
            pipeServer);
    }

    private void WaitForConnectionCallback(IAsyncResult iar)
    {
        Console.WriteLine("Client connected");
        if (iar.AsyncState is null)
            throw new InvalidOperationException(
                "The pipe server is null.");
        try
        {
            var pipeServer = (NamedPipeServerStream)iar.AsyncState;
            pipeServer.EndWaitForConnection(iar);
            Console.WriteLine("Reading 255 bytes of client data...");
            var buffer = new byte[255];
            var dataList = new List<byte>();
            var count = pipeServer
                .Read(buffer, 
                    0, 
                    255);
            dataList.AddRange(buffer.Take(count));
            while (count == 255)
            {
                Console.WriteLine("Reading 255 more bytes of client data...");
                count = pipeServer
                    .Read(buffer, 
                        0, 
                        255);
                dataList.AddRange(buffer.Take(count));
            }
            Console.WriteLine(
                $"Finished reading client data... Length: {dataList.Count}");
            ClientDataHandler?.Invoke(
                MemoryContract<byte[]>
                    .Deserialize(dataList.ToArray()));
            Console.WriteLine("Invoked delegate complete, closing server");
            pipeServer.Close();
            pipeServer = null;
            pipeServer = new NamedPipeServerStream(_pipeName,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
            Console.WriteLine("Pipe server created, waiting for connections...");
            pipeServer.BeginWaitForConnection(
                WaitForConnectionCallback,
                pipeServer);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}