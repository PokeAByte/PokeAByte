using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Utility.BuildMapperBindings
{
    internal class PokeAByteFakeInstance : IPokeAByteInstance
    {
        public Dictionary<string, object?> State => throw new NotImplementedException();

        public List<IClientNotifier> ClientNotifiers => throw new NotImplementedException();

        public IPokeAByteDriver Driver => throw new NotImplementedException();

        public IPokeAByteMapper Mapper => throw new NotImplementedException();

        public IPlatformOptions? PlatformOptions => throw new NotImplementedException();

        public Dictionary<string, object?> Variables => throw new NotImplementedException();

        public event InstanceProcessingAbort? OnProcessingAbort;

        public Task ResetState()
        {
            throw new NotImplementedException();
        }

        public Task Load(IPokeAByteDriver driver, string mapperId)
        {
            throw new NotImplementedException();
        }

        public object? ExecuteExpression(string? expression, object x)
        {
            throw new NotImplementedException();
        }

        public object? ExecuteModuleFunction(string? function, IPokeAByteProperty property)
        {
            throw new NotImplementedException();
        }

        public bool? GetModuleFunctionResult(string? function, IPokeAByteProperty property)
        {
            throw new NotImplementedException();
        }

        public Task StartProcessing()
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
