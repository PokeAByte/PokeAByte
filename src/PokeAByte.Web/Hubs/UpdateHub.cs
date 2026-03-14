using Microsoft.AspNetCore.SignalR;
using PokeAByte.Web.Controllers;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Hubs;

public class UpdateHub(IInstanceService instanceService) : Hub
{
    public override Task OnConnectedAsync()
    {
        var mapper = instanceService.Instance?.Mapper;
        var mapperModel = mapper == null
            ? null
            : new MapperModel
            {
                Meta = MapperMetaModel.FromMapperSection(mapper.Metadata),
                Properties = mapper.Properties.Values,
                Glossary = mapper.References.Values.MapToDictionaryGlossaryItemModel()
            };

        return this.Clients.Caller.SendAsync("Hello", mapperModel);
    }
}