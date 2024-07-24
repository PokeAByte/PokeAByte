using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models.Properties;

namespace PokeAByte.Application.Mappers;

public static class MapperHelper
{
    public static PropertyModel MapToPropertyModel(this IPokeAByteProperty x) =>
        new()
        {
            Path = x.Path,

            Type = x.Type,
            MemoryContainer = x.MemoryContainer,
            Address = x.Address,
            Length = x.Length,
            Size = x.Size,
            Reference = x.Reference,
            Bits = x.Bits,
            Description = x.Description,

            Value = x.Value,
            Bytes = x.Bytes?.ToIntegerArray(),

            IsFrozen = x.IsFrozen,
            IsReadOnly = x.IsReadOnly,
        };

    public static Dictionary<string, IEnumerable<GlossaryItemModel>> MapToDictionaryGlossaryItemModel(
        this IEnumerable<ReferenceItems> glossaryList)
    {
        var dictionary = new Dictionary<string, IEnumerable<GlossaryItemModel>>();

        foreach (var item in glossaryList)
        {
            dictionary[item.Name] = item.Values.Select(x => new GlossaryItemModel()
            {
                Key = x.Key,
                Value = x.Value
            });
        }

        return dictionary;
    }
    
}