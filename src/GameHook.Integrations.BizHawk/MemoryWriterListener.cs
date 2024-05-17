using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace GameHookIntegration;

public delegate void ClientData(byte[] data);

public class MemoryWriterListener : IDisposable
{
    public const string PipeName = "GameHook_BizHawk_Pipe";

    public event ClientData ClientData;
    
    //We only want to listen for one client as of now
    private NamedPipeServerStream? _pipeServer;
    //The result of waiting for a client to connect
    private IAsyncResult? _connectionResult;

    public void StartServer()
    {
        _pipeServer = new(PipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        _connectionResult = _pipeServer.BeginWaitForConnection(HandleClientConnect, _pipeServer);
    }

    public void EndServer()
    {
        if(_connectionResult is not null)
            _pipeServer?.EndWaitForConnection(_connectionResult);
        _pipeServer?.Close();
        _pipeServer?.Dispose();
        _pipeServer = null;
    }

    public void RestartServer()
    {
        EndServer();
        StartServer();
    }
    private void HandleClientConnect(IAsyncResult result)
    {
        if(_pipeServer is null)
            throw new InvalidOperationException("Named pipe server is null.");
        try
        {

            _pipeServer.EndWaitForConnection(result);
            byte[] buffer = new byte[100];

            // Read the incoming message
            //todo: send and receive MemoryContract
            _pipeServer.Read(buffer, 0, 100);
            
            ClientData.Invoke(buffer);
            
            //Restart the piped server
            RestartServer();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public void Dispose()
    {
        EndServer();
    }
}