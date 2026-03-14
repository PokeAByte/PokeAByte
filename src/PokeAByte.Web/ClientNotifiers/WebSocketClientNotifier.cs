using Microsoft.AspNetCore.SignalR;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Web.Controllers;
using PokeAByte.Web.Hubs;
using PokeAByte.Web.Models;

namespace PokeAByte.Web.ClientNotifiers;

public class WebSocketClientNotifier(IHubContext<UpdateHub> hubContext) : IClientNotifier
{
    private readonly IHubContext<UpdateHub> _hubContext = hubContext;

    public Task SendInstanceReset() =>
        _hubContext.Clients.All.SendAsync("InstanceReset");

    public Task SendMapperLoaded(IPokeAByteMapper mapper) =>
        _hubContext.Clients.All.SendAsync(
            "MapperLoaded",
            new MapperModel
            {
                Meta = MapperMetaModel.FromMapperSection(mapper.Metadata),
                Properties = mapper.Properties.Values,
                Glossary = mapper.References.Values.MapToDictionaryGlossaryItemModel()
            }
        );

    public Task SendError(IProblemDetails problemDetails)
    {
        return _hubContext.Clients.All.SendAsync("Error", problemDetails);
    }

    public Task SendPropertiesChanged(IList<IPokeAByteProperty> properties)
    {
        return _hubContext.Clients.All.SendAsync("PropertiesChanged", properties);
    }
}
