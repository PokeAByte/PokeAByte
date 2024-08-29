using Microsoft.AspNetCore.SignalR;
using PokeAByte.Application;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Web.Controllers;

namespace PokeAByte.Web.Hubs
{

    public class UpdateHub(PokeAByteInstance pokeAByte, Domain.Models.AppSettings appSettings) : Hub
    {
        private PokeAByteInstance _instance = pokeAByte;
        private Domain.Models.AppSettings _appSettings = appSettings;
        public delegate void UpdateHubConnectionEvent(ISingleClientProxy newClient);

        public event UpdateHubConnectionEvent? OnConnection;

        public override Task OnConnectedAsync() {
            this.Clients.Caller.SendAsync(
                "Hello", 
                _instance.Mapper == null 
                    ? null
                    : new Controllers.MapperModel {
                        Meta = new Controllers.MapperMetaModel
                        {
                            Id = _instance.Mapper.Metadata.Id,
                            GameName = _instance.Mapper.Metadata.GameName,
                            GamePlatform = _instance.Mapper.Metadata.GamePlatform,
                            MapperReleaseVersion = _appSettings.MAPPER_VERSION
                        },
                        Properties = _instance.Mapper.Properties.Values.Select(x => x.MapToPropertyModel()).ToArray(),
                        Glossary = _instance.Mapper.References.Values.MapToDictionaryGlossaryItemModel()
                    }
            );
            return Task.CompletedTask;
        }
    }
}