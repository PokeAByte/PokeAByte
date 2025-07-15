using System.IO.Pipes;
using PokeAByte.Domain.Models;

namespace PokeAByte.Infrastructure.Drivers.Bizhawk;

public static class BizhawkNamedPipesClient
{
    public const string PipeName = "BizHawk_Named_Pipe";

    public static void WriteToBizhawk(MemoryContract<byte[]> contract, int timeoutMs = 100)
    {
        try
        {
            NamedPipeClientStream client = new(".",
                PipeName,
                PipeDirection.Out,
                PipeOptions.Asynchronous);
            var contractBytes = contract.Serialize();
            client.Connect(timeoutMs);
            client.BeginWrite(contractBytes,
                0,
                contractBytes.Length,
                SendAsync,
                client
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    private static void SendAsync(IAsyncResult iar)
    {
        if (iar.AsyncState is null)
            throw new InvalidOperationException("Named pipe client is null.");
        try
        {
            var pipeClient = (NamedPipeClientStream)iar.AsyncState;
            pipeClient.EndWrite(iar);
            pipeClient.Flush();
            pipeClient.Close();
            pipeClient.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}