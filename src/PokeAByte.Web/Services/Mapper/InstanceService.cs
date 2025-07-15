using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Logic;

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
    IClientNotifier clientNotifier
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
            _instance = new PokeAByteInstance(instanceLogger, scriptConsoleAdapter, clientNotifier, MapperContent, driver);
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

    private Task HandleProcessingAbort()
    {
        if (_instance != null)
        {
            _ = Task.Run(async () =>
            {

                await _instance.DisposeAsync();
                _instance = null;
            });
        }
        return Task.CompletedTask;
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
