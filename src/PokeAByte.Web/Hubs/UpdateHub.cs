using Microsoft.AspNetCore.SignalR;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Web.Controllers;

namespace PokeAByte.Web.Hubs;

public class UpdateHub(IPokeAByteInstance instance, Domain.Models.AppSettings appSettings) : Hub
{
    public override Task OnConnectedAsync()
    {
        var mapperModel = instance.Mapper == null
            ? null
            : new MapperModel
            {
                Meta = new MapperMetaModel
                {
                    Id = instance.Mapper.Metadata.Id,
                    GameName = instance.Mapper.Metadata.GameName,
                    GamePlatform = instance.Mapper.Metadata.GamePlatform,
                    MapperReleaseVersion = appSettings.MAPPER_VERSION
                },
                Properties = instance.Mapper.Properties.Values.Select(x => x.MapToPropertyModel()).ToArray(),
                Glossary = instance.Mapper.References.Values.MapToDictionaryGlossaryItemModel()
            };

        return this.Clients.Caller.SendAsync("Hello", mapperModel);
    }
}