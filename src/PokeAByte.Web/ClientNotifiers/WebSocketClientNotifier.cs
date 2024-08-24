using Microsoft.AspNetCore.SignalR;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Web.Hubs;

namespace PokeAByte.Web.ClientNotifiers
{
	public class WebSocketClientNotifier(ILogger<WebSocketClientNotifier> logger, IHubContext<UpdateHub> hubContext) : IClientNotifier
	{
        private static TimeSpan BatchTime = TimeSpan.FromMilliseconds(50);
		private readonly ILogger<WebSocketClientNotifier> _logger = logger;
		private readonly IHubContext<UpdateHub> _hubContext = hubContext;
		private DateTime _lastUpdate = DateTime.MinValue;
        private List<IPokeAByteProperty> _batch = [];

		public Task SendInstanceReset() =>
			_hubContext.Clients.All.SendAsync("InstanceReset");

		public Task SendMapperLoaded(IPokeAByteMapper mapper) =>
			_hubContext.Clients.All.SendAsync("MapperLoaded");

		public Task SendError(IProblemDetails problemDetails) =>
			_hubContext.Clients.All.SendAsync("Error", problemDetails);

		public event PropertyChangedEventHandler? PropertyChangedEvent;
		

		public async Task SendPropertiesChanged(IEnumerable<IPokeAByteProperty> properties)
		{
            /*
            To avoid constantly re-rendering in the frontend (and all the work here on the server side),
            we wait at least some amount of time (>16ms) before sending property updates to the client(s).
            We store the property data in the _batch list until then, removing old entries for the same path 
            (so we don't end up sending 300 property updates in one go with 99% of them being irrelevant).
            */
			if (DateTime.Now - this._lastUpdate < BatchTime)
			{
				_batch.RemoveAll(batched => properties.Any(property => property.Path == batched.Path));
				_batch.AddRange(properties);
				return;
			}
			
			_lastUpdate = DateTime.Now;
			var props = _batch.ToList();
			await _hubContext.Clients.All.SendAsync(
				"PropertiesChanged",
				props.Select(x => new
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
					bytes = x.Bytes != null 
						? Array.ConvertAll(x.Bytes, value => (int)value) 
						: null,
					isFrozen = x.IsFrozen,
					isReadOnly = x.IsReadOnly,
					fieldsChanged = x.FieldsChanged
				}
				).ToArray()
			);
			OnPropertyChangedEvent(new PropertyChangedEventArgs(props));
			_batch.Clear();
		}

		protected virtual void OnPropertyChangedEvent(PropertyChangedEventArgs args)
		{
			PropertyChangedEventHandler? handler = PropertyChangedEvent;
			handler?.Invoke(this, args);
		}
	}
}
