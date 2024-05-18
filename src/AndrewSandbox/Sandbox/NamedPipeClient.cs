using System.IO.Pipes;
using GameHook.Contracts;

namespace Sandbox;

public class NamedPipeClient
{
    public void OpenClientPipe(string  pipeName, MemoryContract<byte[]> contact, int timeoutMs = 100)
    {
        try
        {
            NamedPipeClientStream client = new(".", 
                pipeName,
                PipeDirection.Out,
                PipeOptions.Asynchronous);
            client.Connect(timeoutMs);
            var serializeData = contact.Serialize();
            client.BeginWrite(serializeData, 
                0, 
                serializeData.Length, 
                SendAsync, 
                client);  
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void SendAsync(IAsyncResult iar)
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