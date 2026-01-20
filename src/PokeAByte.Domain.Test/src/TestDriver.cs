using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Test;

public record DriverWrite(uint Address, byte[] Bytes);

public class TestDriver : IPokeAByteDriver
{
    private byte[] _data;
    internal IEnumerable<byte> Bytes => _data.AsReadOnly();

    public string ProperName { get; } = "TestDriver";
    public int DelayMsBetweenReads { get; } = 0;
    public List<DriverWrite> Writes { get; } = [];
    public bool SupportsFreeze { get; } = false;
    private Lock _lock = new();

    public TestDriver(byte[] data)
    {
        _data = data;
    }

    public void SetData(byte[] newData)
    {
        if (newData.Length != _data.Length)
        {
            throw new ArgumentException("New data has the wrong length.");
        }
        _data = newData;
    }

    public Task Disconnect() => Task.CompletedTask;
    public Task EstablishConnection() => Task.CompletedTask;

    public ValueTask ReadBytes(BlockData[] transferBlocks)
    {
        lock (_lock)
        {
            foreach (var block in transferBlocks)
            {
                _data.AsSpan().CopyTo(block.Data.Span);
            }
            return ValueTask.CompletedTask;
        }
    }

    public ValueTask WriteBytes(uint address, byte[] values, string path = null)
    {
        lock (_lock)
        {
            values.CopyTo(_data.AsSpan((int)address, values.Length));
            this.Writes.Add(new(address, values));
            return ValueTask.CompletedTask;
        }
    }

    public bool LastWriteMatches(byte[] bytes, int? address = null)
    {
        lock (_lock)
        {
            return Writes.Count > 0
                ? Writes.Last().Bytes.SequenceEqual(bytes)
                : false;
        }
    }
}
