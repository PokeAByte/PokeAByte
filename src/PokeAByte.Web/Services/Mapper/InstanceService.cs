using PokeAByte.Application;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Web.Services.Mapper;

public interface IInstanceService
{
    IPokeAByteInstance? Instance { get; }

    Task LoadMapper(MapperContent MapperContent, IPokeAByteDriver driver);
    Task StopProcessing();
}

public class InstanceService(
    ILogger<IInstanceService> logger,
    ILogger<PokeAByteInstance> instanceLogger,
    ScriptConsole scriptConsoleAdapter,
    IEnumerable<IClientNotifier> clientNotifiers
) : IInstanceService
{
    private IPokeAByteInstance? _instance = null;
    public IPokeAByteInstance? Instance => _instance;

    public async Task LoadMapper(MapperContent MapperContent, IPokeAByteDriver driver)
    {
        logger.LogDebug("Creating PokeAByte mapper instance...");
        if (_instance != null)
        {
            await _instance.DisposeAsync();
        }
        try
        {
            _instance = new PokeAByteInstance(instanceLogger, scriptConsoleAdapter, clientNotifiers, MapperContent, driver);
            _instance.OnProcessingAbort += HandleProcessingAbort;
            await _instance.StartProcessing();
        }
        catch
        {
            if (_instance != null)
            {
                await _instance.DisposeAsync();
            }
            throw;
        }
    }

    private async Task HandleProcessingAbort()
    {
        if (_instance != null)
        {
            await _instance.DisposeAsync();
            _instance = null;
        }
    }

    public async Task StopProcessing()
    {
        if (_instance != null)
        {
            await _instance.DisposeAsync();
            _instance = null;
        }
    }
}
