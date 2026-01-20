using System.Collections.Generic;
using System.Threading.Tasks;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Test;

public class TestClientNotifier : IClientNotifier
{
    private readonly List<IProblemDetails> errors = [];

    public IEnumerable<IProblemDetails> Errors => errors.AsReadOnly();
    public int InstanceResets { get; private set; } = 0;
    public List<IPokeAByteMapper> LoadedMappers { get; } = [];
    public List<IPokeAByteProperty> PropertyChanges { get; } = [];
    public Task SendError(IProblemDetails problemDetails)
    {
        errors.Add(problemDetails);
        return Task.CompletedTask;
    }

    public Task SendInstanceReset()
    {
        InstanceResets++;
        return Task.CompletedTask;
    }

    public Task SendMapperLoaded(IPokeAByteMapper mapper)
    {
        LoadedMappers.Add(mapper);
        return Task.CompletedTask;
    }

    public Task SendPropertiesChanged(IList<IPokeAByteProperty> properties)
    {
        PropertyChanges.Clear();
        PropertyChanges.AddRange(properties);
        return Task.CompletedTask;
    }

    public void ResetChanges()
    {
        PropertyChanges.Clear(); ;
    }
}
