using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Models.Properties;

namespace PokeAByte.Web.Controllers;

static class MapperHelper
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
            BaseProperty = x,
        };

    public static Dictionary<string, IEnumerable<GlossaryItemModel>> MapToDictionaryGlossaryItemModel(
        this IEnumerable<ReferenceItems> glossaryList)
    {
        var dictionary = new Dictionary<string, IEnumerable<GlossaryItemModel>>();

        foreach (var item in glossaryList)
        {
            dictionary[item.Name] = item.Values.Select(x => new GlossaryItemModel(x.Key, x.Value));
        }

        return dictionary;
    }
}

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[Route("mapper")]
public class MapperController : ControllerBase
{

    public IPokeAByteInstance Instance { get; }
    private readonly AppSettings _appSettings;
    public readonly IBizhawkMemoryMapDriver _bizhawkMemoryMapDriver;
    public readonly IRetroArchUdpPollingDriver _retroArchUdpPollingDriver;
    public readonly IStaticMemoryDriver _staticMemoryDriver;

    public MapperController(
        IPokeAByteInstance pokeAByteInstance,
        AppSettings appSettings,
        IBizhawkMemoryMapDriver bizhawkMemoryMapDriver,
        IRetroArchUdpPollingDriver retroArchUdpPollingDriver,
        IStaticMemoryDriver nullDriver)
    {
        Instance = pokeAByteInstance;

        _appSettings = appSettings;
        _bizhawkMemoryMapDriver = bizhawkMemoryMapDriver;
        _retroArchUdpPollingDriver = retroArchUdpPollingDriver;
        _staticMemoryDriver = nullDriver;
    }

    [HttpGet]
    public ActionResult<MapperModel> GetMapper()
    {
        if (Instance.Initalized == false || Instance.Mapper == null)
            return ApiHelper.MapperNotLoaded();

        var model = new MapperModel()
        {
            Meta = new MapperMetaModel()
            {
                Id = Instance.Mapper.Metadata.Id,
                GameName = Instance.Mapper.Metadata.GameName,
                GamePlatform = Instance.Mapper.Metadata.GamePlatform,
                MapperReleaseVersion = _appSettings.MAPPER_VERSION
            },
            Properties = Instance.Mapper.Properties.Values.Select(x => x.MapToPropertyModel()).ToArray(),
            Glossary = Instance.Mapper.References.Values.MapToDictionaryGlossaryItemModel()
        };

        return Ok(model);
    }

    [HttpPut]
    public async Task<ActionResult> ChangeMapper(MapperReplaceModel model)
    {
        if (model.Driver == "bizhawk")
        {
            await Instance.Load(_bizhawkMemoryMapDriver, model.Id);
        }
        else if (model.Driver == "retroarch")
        {
            await Instance.Load(_retroArchUdpPollingDriver, model.Id);
        }
        else if (model.Driver == "staticMemory")
        {
            await Instance.Load(_staticMemoryDriver, model.Id);
        }
        else
        {
            return ApiHelper.BadRequestResult("A valid driver was not supplied.");
        }

        return Ok();
    }

    [HttpGet("meta")]
    public ActionResult<MapperMetaModel> GetMeta()
    {
        if (Instance.Initalized == false || Instance.Mapper == null)
            return ApiHelper.MapperNotLoaded();

        var meta = Instance.Mapper.Metadata;
        var model = new MapperMetaModel
        {
            Id = meta.Id,
            GameName = meta.GameName,
            GamePlatform = meta.GamePlatform,
            MapperReleaseVersion = _appSettings.MAPPER_VERSION
        };

        return Ok(model);
    }

    [HttpGet("values/{**path}/")]
    [Produces("text/plain")]
    public ActionResult GetValueAsync(string path)
    {
        if (Instance.Initalized == false || Instance.Mapper == null)
            return ApiHelper.MapperNotLoaded();

        path = path.StripEndingRoute().FromRouteToPath();

        var prop = Instance.Mapper.Properties[path];

        if (prop == null)
        {
            return NotFound();
        }

        if (prop.Value != null && prop.Value is string == false && prop.Value is int == false)
        {
            return BadRequest($"{prop.Path} is an object and cannot be converted to text.");
        }

        return Ok(prop.Value?.ToString() ?? string.Empty);
    }

    [HttpGet("properties")]
    public ActionResult<IEnumerable<PropertyModel>> GetProperties()
    {
        if (Instance.Initalized == false || Instance.Mapper == null)
            return ApiHelper.MapperNotLoaded();

        return Ok(Instance.Mapper.Properties.Values.Select(x => x.MapToPropertyModel()));
    }

    [HttpGet("properties/{**path}/")]
    public ActionResult<PropertyModel?> GetProperty(string path)
    {
        if (Instance.Initalized == false || Instance.Mapper == null)
            return ApiHelper.MapperNotLoaded();

        path = path.StripEndingRoute().FromRouteToPath();

        var prop = Instance.Mapper.Properties[path];

        if (prop == null)
        {
            return NotFound();
        }

        return Ok(prop.MapToPropertyModel());
    }

    [HttpPost("set-property-value")]
    public async Task<ActionResult> UpdatePropertyValueAsync(UpdatePropertyValueModel model)
    {
        Console.WriteLine(Instance.Initalized);
        Console.WriteLine(Instance.Mapper != null);
        if (Instance.Initalized == false || Instance.Mapper == null)
            return ApiHelper.MapperNotLoaded();

        var path = model.Path.StripEndingRoute().FromRouteToPath();

        var prop = Instance.Mapper.Properties[path];

        if (prop == null)
        {
            return NotFound();
        }

        if (prop.IsReadOnly)
        {
            return ApiHelper.BadRequestResult("Property is read only.");
        }

        await prop.WriteValue(model.Value?.ToString() ?? string.Empty, model.Freeze);

        return Ok();
    }

    [HttpPost("set-properties-by-bits")]
    public async Task<ActionResult> UpdatePropertyByBitsAsync(List<UpdatePropertyValueModel> model)
    {
        if (Instance.Initalized == false || Instance.Mapper == null)
            return ApiHelper.MapperNotLoaded();

        if (model.Count == 0)
            return ApiHelper.BadRequestResult("Properties count is zero.");
        //Make sure all values exist
        if (model.Any(x => x.Value is null || string.IsNullOrEmpty(x.Value!.ToString())))
            return ApiHelper.BadRequestResult("Values cannot be null.");
        //Convert BitProperty to GameHookBitProperty
        var gameHookProperties = model
            .Select(x =>
            {
                var path = x.Path.StripEndingRoute().FromRouteToPath();
                //Since we are already checking if the value is null, we shouldn't need to worry
                //about it being string.Empty. I just want to stop the compiler from complaining 
                return new KeyValuePair<IPokeAByteProperty, string>
                    (Instance.Mapper.Properties[path], x.Value?.ToString() ?? string.Empty);
            })
            .ToDictionary();
        //Make sure the addresses are the same
        var baseAddress = gameHookProperties.First().Key.Address;
        var baseType = gameHookProperties.First().Key.Type;
        var baseLength = gameHookProperties.First().Key.Length;
        if (baseAddress is null || baseLength is null)
            return ApiHelper.BadRequestResult("Address or length for property is null.");
        if (gameHookProperties.Any(x =>
                    x.Key.Address != baseAddress.Value ||
                    x.Key.Type != baseType ||
                    x.Key.Length != baseLength))
            return ApiHelper.BadRequestResult("Addresses or types for the properties are not the same.");

        try
        {
            //Construct the updated byte array from value
            var outputByteArray = MultiplePropertiesUpdater
                .ConstructUpdatedBytes(gameHookProperties);
            //Update only the first property since WriteBytes will overwrite the entire address space 
            //of this property. We are maintaining the original set of bytes before overwriting only
            //the values 
            await gameHookProperties
                .First()
                .Key
                .WriteBytes(outputByteArray, false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ApiHelper.BadRequestResult(e.ToString());
        }
        return Ok();
    }

    [HttpPost("set-property-bytes")]
    public async Task<ActionResult> UpdatePropertyBytesAsync(UpdatePropertyBytesModel model)
    {
        if (Instance.Initalized == false || Instance.Mapper == null)
            return ApiHelper.MapperNotLoaded();

        var path = model.Path.StripEndingRoute().FromRouteToPath();
        var actualBytes = Array.ConvertAll(model.Bytes, x => (byte)x);

        var prop = Instance.Mapper.Properties[path];

        if (prop == null)
        {
            return NotFound();
        }

        if (prop.IsReadOnly)
        {
            return ApiHelper.BadRequestResult("Property is read only.");
        }

        await prop.WriteBytes(actualBytes, model.Freeze);

        return Ok();
    }

    [HttpPost("set-property-frozen")]
    public async Task<ActionResult> FreezePropertyAsync(UpdatePropertyFreezeModel model)
    {
        if (Instance.Initalized == false || Instance.Mapper == null)
            return ApiHelper.MapperNotLoaded();

        var path = model.Path.StripEndingRoute().FromRouteToPath();

        var prop = Instance.Mapper.Properties[path];

        if (prop == null)
        {
            return NotFound();
        }

        if (prop.IsReadOnly)
        {
            return ApiHelper.BadRequestResult("Property is read only.");
        }

        if (model.Freeze)
        {
            await prop.FreezeProperty(prop.Bytes ?? Array.Empty<byte>());
        }
        else
        {
            await prop.UnfreezeProperty();
        }

        return Ok();
    }

    [HttpGet("glossary")]
    public ActionResult<Dictionary<string, Dictionary<string, GlossaryItemModel>>> GetGlossary()
    {
        if (Instance.Initalized == false || Instance.Mapper == null)
            return ApiHelper.MapperNotLoaded();

        return Ok(Instance.Mapper.References.Values.MapToDictionaryGlossaryItemModel());
    }

    [HttpGet("glossary/{key}")]
    public ActionResult<IEnumerable<GlossaryItemModel>> GetGlossaryPage(string key)
    {
        if (Instance.Initalized == false || Instance.Mapper == null)
            return ApiHelper.MapperNotLoaded();

        key = key.StripEndingRoute();

        var glossaryItem = Instance.Mapper.References[key];
        if (glossaryItem == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(glossaryItem.Values.Select(x => new GlossaryItemModel(x.Key, x.Value)));
        }
    }
}
