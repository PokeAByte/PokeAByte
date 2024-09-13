using Microsoft.AspNetCore.SignalR;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Web.Controllers;
using PokeAByte.Web.Hubs;

namespace PokeAByte.Web.ClientNotifiers
{
    public class WebSocketClientNotifier(
        AppSettings _appSettings,
        ILogger<WebSocketClientNotifier> logger, 
        IHubContext<UpdateHub> hubContext) : IClientNotifier
    {
        private readonly ILogger<WebSocketClientNotifier> _logger = logger;
        private readonly IHubContext<UpdateHub> _hubContext = hubContext;

        public Task SendInstanceReset() =>
            _hubContext.Clients.All.SendAsync("InstanceReset");

        public Task SendMapperLoaded(IPokeAByteMapper mapper) =>
            _hubContext.Clients.All.SendAsync(
                "MapperLoaded", 
                new MapperModel {
                    Meta = new MapperMetaModel
                    {
                        Id = mapper.Metadata.Id,
                        GameName = mapper.Metadata.GameName,
                        GamePlatform = mapper.Metadata.GamePlatform,
                        MapperReleaseVersion = _appSettings.MAPPER_VERSION
                    },
                    Properties = mapper.Properties.Values.Select(x => x.MapToPropertyModel()).ToArray(),
                    Glossary = mapper.References.Values.MapToDictionaryGlossaryItemModel()
                }
            );

        public Task SendError(IProblemDetails problemDetails) =>
            _hubContext.Clients.All.SendAsync("Error", problemDetails);

        public event PropertyChangedEventHandler? PropertyChangedEvent;
        public async Task SendPropertiesChanged(IEnumerable<IPokeAByteProperty> properties)
        {
            var props = properties.ToList();
            await _hubContext.Clients.All.SendAsync("PropertiesChanged", props.Select(x => new
            {
                path = x.Path,
                memoryContainer = x.MemoryContainer,
                address = x.Address,
                length = x.Length,
                size = x.Size,
                reference = x.Reference,
                bits = x.Bits,
                description = x.Description,
                value = x.Value,
                bytes = x.Bytes?.ToIntegerArray(),

                isFrozen = x.IsFrozen,
                isReadOnly = x.IsReadOnly,

                fieldsChanged = x.FieldsChanged
            }).ToArray());
            OnPropertyChangedEvent(new PropertyChangedEventArgs(props));
        }

        protected virtual void OnPropertyChangedEvent(PropertyChangedEventArgs args)
        {
            PropertyChangedEventHandler? handler = PropertyChangedEvent;
            handler?.Invoke(this, args);
        }
    }
}
