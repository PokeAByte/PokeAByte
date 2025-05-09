namespace PokeAByte.Domain.Interfaces;

public interface IPokeAByteInstance : IAsyncDisposable
{
    Dictionary<string, object?> State { get; }
    Dictionary<string, object?> Variables { get; }

    IClientNotifier ClientNotifier { get; }
    IPokeAByteDriver Driver { get; }
    IPokeAByteMapper Mapper { get; }
    Task StartProcessing();

    object? ExecuteExpression(string expression, object x);
    object? ExecuteModuleFunction(string function, IPokeAByteProperty property);

    bool? GetModuleFunctionResult(string function, IPokeAByteProperty property);
    public event InstanceProcessingAbort? OnProcessingAbort;
}

public delegate Task InstanceProcessingAbort();