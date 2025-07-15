using Microsoft.AspNetCore.SignalR;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Web.Controllers;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Hubs;

public class UpdateHub(IInstanceService instanceService, Domain.Models.AppSettings appSettings) : Hub
{
    public override Task OnConnectedAsync()
    {
        var mapper = instanceService.Instance?.Mapper;
        var mapperModel = mapper == null
            ? null
            : new MapperModel
            {
                Meta = new MapperMetaModel
                {
                    Id = mapper.Metadata.Id,
                    FileId = mapper.Metadata.FileId,
                    GameName = mapper.Metadata.GameName,
                    GamePlatform = mapper.Metadata.GamePlatform,
                    MapperReleaseVersion = appSettings.MAPPER_VERSION
                },
                Properties = mapper.Properties.Values,
                Glossary = mapper.References.Values.MapToDictionaryGlossaryItemModel()
            };

        return this.Clients.Caller.SendAsync("Hello", mapperModel);
    }
}