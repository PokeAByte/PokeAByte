namespace PokeAByte.Domain.Interfaces;

public interface IPokeAByteInstance
{
    bool Initalized { get; }
    Dictionary<string, object?> State { get; }
    Dictionary<string, object?> Variables { get; }

    List<IClientNotifier> ClientNotifiers { get; }
    IPokeAByteDriver? Driver { get; }
    IPokeAByteMapper? Mapper { get; }
    Task ResetState();
    Task Load(IPokeAByteDriver driver, string mapperId);

    object? ExecuteExpression(string? expression, object x);
    object? ExecuteModuleFunction(string? function, IPokeAByteProperty property);

    bool? GetModuleFunctionResult(string? function, IPokeAByteProperty property);
}
