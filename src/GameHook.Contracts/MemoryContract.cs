namespace GameHook.Contracts;

public record MemoryContract<T>
{
    public long MemoryAddressStart { get; init; } = 0x0L;
    public int DataLength { get; init; } = 0;
    public string DataType => typeof(T).Name;
    public T? Data { get; init; }
}
