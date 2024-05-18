using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using GameHook.Contracts;
using GameHook.Contracts.Generation3;
using Sandbox;
using InvalidOperationException = System.InvalidOperationException;

namespace GameHook.UnitTests;

[TestClass]
public class BizHawkNamedPipeServerTest
{
    //Todo: better test coverage
    [TestMethod]
    public void NamedPipe_Connection_Does_Not_Throw_Exception()
    {
        var success = SendData();
        Assert.IsTrue(success, "Method throws an exception.");
    }

    [TestMethod]
    public void MemoryContract_ByteArray_Test()
    {
        var data = new byte[16];
        MemoryContract<byte[]> memoryContract = new()
        {
            Data = data,
            DataLength = data.Length,
            MemoryAddressStart = 0x0L
        };
        Assert.IsTrue(memoryContract.DataLength == 16, "Sizes do not match.");
    }

    [TestMethod]
    public void LoadMemory_CreatePokemonStructure_UpdateStructure_SendThroughPipe()
    {
        try
        {
            NamedPipeClient client = new();
            var biz = new BizHawkHelper();
            var slotOneData = biz.GetPokemonFromParty(0);
            var pokeStruct = PokemonStructure.Create(slotOneData);
            pokeStruct.GrowthSubstructure.Species = 0xF7;
            pokeStruct.UpdateChecksum();
            var pokeBytes = pokeStruct.AsByteArray();
            var contract = new MemoryContract<byte[]>()
            {
                Data = pokeBytes,
                DataLength = pokeBytes.Length,
                MemoryAddressStart = 0x244EC,
                BizHawkIdentifier = "EWRAM"
            };
            client.OpenClientPipe("BizHawk_Named_Pipe", contract);
            //create the pipe
            /*NamedPipeClientStream pipeStream = new NamedPipeClientStream
                (".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            pipeStream.Connect(100);

            //Connect to bizhawk and get poke data

            pipeStream.BeginWrite
                (buffer, 0, buffer.Length, AsyncSend, pipeStream);*/
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    private const string _pipeName = "BizHawk_Named_Pipe";
    private bool SendData()
    {
        try
        {
            NamedPipeClientStream pipeStream = new NamedPipeClientStream
                (".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

            // The connect function will indefinitely wait for the pipe to become available
            // If that is not acceptable specify a maximum waiting time (in ms)
            pipeStream.Connect(100);
            Debug.WriteLine("[Client] Pipe connection established");

            byte[] buffer = Encoding.UTF8.GetBytes("Hello, world");
            pipeStream.BeginWrite
                (buffer, 0, buffer.Length, AsyncSend, pipeStream);
        }
        catch (TimeoutException oEX)
        {
            Debug.WriteLine(oEX.Message);
            throw;
        }

        return true;
    }
    private void AsyncSend(IAsyncResult iar)
    {
        try
        {
            // Get the pipe
            NamedPipeClientStream pipeStream = (NamedPipeClientStream)iar.AsyncState;

            // End the write
            pipeStream.EndWrite(iar);
            pipeStream.Flush();
            pipeStream.Close();
            pipeStream.Dispose();
        }
        catch (Exception oEX)
        {
            Debug.WriteLine(oEX.Message);
            throw;
        }
    }
}