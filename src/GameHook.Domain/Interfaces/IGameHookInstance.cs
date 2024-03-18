namespace GameHook.Domain.Interfaces
{
    public interface IGameHookInstance
    {
        bool Initalized { get; }
        Dictionary<string, object?> State { get; }
        Dictionary<string, object?> Variables { get; }

        List<IClientNotifier> ClientNotifiers { get; }
        IGameHookDriver? Driver { get; }
        IGameHookMapper? Mapper { get; }
        IPlatformOptions? PlatformOptions { get; }

        Task Load(IGameHookDriver driver, string mapperId);

        object? ExecuteExpression(string? expression, object x);
        object? ExecuteModuleFunction(string? function, IGameHookProperty property);

        bool? GetModuleFunctionResult(string? function, IGameHookProperty property);
    }
}
