using GameHook.Domain.Models.Mappers;

namespace PokeAByte.Web.Models;

public class VisualMapperDto(MapperDto mapper)
{
    public MapperDto MapperDto { get; init; } = mapper;
    public bool IsSelected { get; set; } = false;
}