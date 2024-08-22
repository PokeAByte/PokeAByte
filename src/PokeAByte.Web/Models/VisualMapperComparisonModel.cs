using PokeAByte.Domain.Models.Mappers;

namespace PokeAByte.Web.Models;

public class VisualMapperComparisonModel(MapperComparisonDto context)
{
    public MapperComparisonDto MapperComparison { get; init; } = context;
    public bool IsSelected { get; set; }
}