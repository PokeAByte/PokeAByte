using GameHook.Domain;
using GameHook.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using PokeAByte.Web.Hubs;

namespace PokeAByte.Web.ClientNotifiers
{
    public class WebSocketClientNotifier(ILogger<WebSocketClientNotifier> logger, IHubContext<UpdateHub> hubContext) : IClientNotifier
    {
        private readonly ILogger<WebSocketClientNotifier> _logger = logger;
        private readonly IHubContext<UpdateHub> _hubContext = hubContext;

        public Task SendInstanceReset() =>
            _hubContext.Clients.All.SendAsync("InstanceReset");

        public Task SendMapperLoaded(IGameHookMapper mapper) =>
            _hubContext.Clients.All.SendAsync("MapperLoaded");

        public Task SendError(IProblemDetails problemDetails) =>
            _hubContext.Clients.All.SendAsync("Error", problemDetails);

        public event PropertyChangedEventHandler? PropertyChangedEvent;
        public async Task SendPropertiesChanged(IEnumerable<IGameHookProperty> properties)
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
                bytes = x.Bytes?.Select(x => (int)x).ToArray(),

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
