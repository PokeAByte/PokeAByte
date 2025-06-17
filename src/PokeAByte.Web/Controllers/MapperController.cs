using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Models.Properties;
using PokeAByte.Web.Services.Drivers;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Controllers;

static class MapperHelper
{
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

public static class MapperEndpoints
{
    public static void MapMapperEndpoints(this WebApplication app)
    {
        app.MapGet("/mapper", GetMapper);
        app.MapPut("/mapper", PutMapper);
        app.MapGet("/mapper/meta", GetMeta);
        app.MapGet("/mapper/properties", GetProperties);
        app.MapGet("/mapper/properties/{**path}/", GetProperty);
        app.MapGet("/mapper/glossary", GetGlossary);
        app.MapGet("/mapper/glossary/{key}", GetGlossaryPage);
        app.MapGet("/mapper/values/{**path}/", GetValueAsync);
        app.MapPost("/mapper/set-property-frozen", FreezePropertyAsync);
        app.MapPost("/mapper/set-property-bytes", UpdatePropertyBytesAsync);
        app.MapPost("/mapper/set-properties-by-bits", UpdatePropertyByBitsAsync);
        app.MapPost("/mapper/set-property-value", UpdatePropertyValueAsync);
    }

    public static IResult GetMapper(IInstanceService instanceService, AppSettings appSettings)
    {
        if (instanceService.Instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        var model = new MapperModel()
        {
            Meta = new MapperMetaModel()
            {
                Id = instanceService.Instance.Mapper.Metadata.Id,
                GameName = instanceService.Instance.Mapper.Metadata.GameName,
                GamePlatform = instanceService.Instance.Mapper.Metadata.GamePlatform,
                MapperReleaseVersion = appSettings.MAPPER_VERSION
            },
            Properties = instanceService.Instance.Mapper.Properties.Values,
            Glossary = instanceService.Instance.Mapper.References.Values.MapToDictionaryGlossaryItemModel()
        };
        return TypedResults.Ok(model);
    }

    public static async Task<IResult> PutMapper(
        IMapperFileService mapperFileService,
        IDriverService driverService,
        IInstanceService instanceService,
        [FromBody] MapperReplaceModel model)
    {
        var mapperContent = await mapperFileService.LoadContentAsync(model.Id);
        switch (model.Driver)
        {
            case DriverModels.Bizhawk:
                await instanceService.LoadMapper(mapperContent, await driverService.GetBizhawkDriver());
                break;
            case DriverModels.RetroArch:
                await instanceService.LoadMapper(mapperContent, await driverService.GetRetroArchDriver());
                break;
            case DriverModels.StaticMemory:
                await instanceService.LoadMapper(mapperContent, driverService.StaticMemory);
                break;
            default:
                return TypedResults.BadRequest("A valid driver was not supplied.");
        }
        return TypedResults.Ok();
    }

    public static IResult GetMeta(IInstanceService instanceService, AppSettings appSettings)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        var meta = instance.Mapper.Metadata;
        var model = new MapperMetaModel
        {
            Id = meta.Id,
            GameName = meta.GameName,
            GamePlatform = meta.GamePlatform,
            MapperReleaseVersion = appSettings.MAPPER_VERSION
        };
        return TypedResults.Ok(model);
    }

    public static IResult GetProperties(IInstanceService instanceService)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        return TypedResults.Ok(instance.Mapper.Properties.Values);
    }

    public static IResult GetProperty(IInstanceService instanceService, [FromRoute] string path)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());
        path = path.StripEndingRoute();

        var prop = instance.Mapper.Properties[path];
        if (prop == null)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(prop);
    }

    public static IResult GetValueAsync(IInstanceService instanceService, string path)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        path = path.StripEndingRoute();
        var prop = instance.Mapper.Properties[path];
        if (prop == null)
        {
            return TypedResults.NotFound();
        }
        if (prop.Value != null && prop.Value is string == false && prop.Value is int == false)
        {
            return TypedResults.BadRequest($"{prop.Path} is an object and cannot be converted to text.");
        }
        return Results.Text(prop.Value?.ToString() ?? string.Empty);
    }

    public static async Task<IResult> FreezePropertyAsync(
        IInstanceService instanceService,
        [FromBody] UpdatePropertyFreezeModel model)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        var path = model.Path.StripEndingRoute();

        var prop = instance.Mapper.Properties[path];

        if (prop == null)
        {
            return TypedResults.NotFound();
        }

        if (prop.IsReadOnly)
        {
            return TypedResults.BadRequest("Property is read only.");
        }

        if (model.Freeze)
        {
            await instance.FreezeProperty(prop, prop.Bytes);
        }
        else
        {
            await instance.UnfreezeProperty(prop);
        }
        return TypedResults.Ok();
    }

    public static IResult GetGlossary(IInstanceService instanceService)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        return TypedResults.Ok(instance.Mapper.References.Values.MapToDictionaryGlossaryItemModel());
    }

    public static IResult GetGlossaryPage(IInstanceService instanceService, [FromRoute] string key)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        key = key.StripEndingRoute();

        var glossaryItem = instance.Mapper.References[key];
        if (glossaryItem == null)
        {
            return TypedResults.NotFound();
        }
        else
        {
            return TypedResults.Ok(glossaryItem.Values.Select(x => new GlossaryItemModel(x.Key, x.Value)));
        }
    }

    public static async Task<IResult> UpdatePropertyValueAsync(IInstanceService instanceService, [FromBody] UpdatePropertyValueModel model)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        var path = model.Path.StripEndingRoute();
        if (path.Contains('/'))
        {
            path = path.Replace('/', '.');
        }

        var prop = instance.Mapper.Properties[path];

        if (prop == null)
        {
            return TypedResults.NotFound();
        }

        if (prop.IsReadOnly)
        {
            return TypedResults.BadRequest("Property is read only.");
        }

        await instance.WriteValue(prop, model.Value?.ToString() ?? string.Empty, model.Freeze);

        return TypedResults.Ok();
    }

    public static async Task<IResult> UpdatePropertyByBitsAsync(IInstanceService instanceService, List<UpdatePropertyValueModel> model)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        if (model.Count == 0)
            return TypedResults.BadRequest("Properties count is zero.");
        //Make sure all values exist
        if (model.Any(x => x.Value is null || string.IsNullOrEmpty(x.Value!.ToString())))
            return TypedResults.BadRequest("Values cannot be null.");
        //Convert BitProperty to GameHookBitProperty
        var gameHookProperties = model
            .Select(x =>
            {
                var path = x.Path.StripEndingRoute().FromRouteToPath();
                //Since we are already checking if the value is null, we shouldn't need to worry
                //about it being string.Empty. I just want to stop the compiler from complaining 
                return new KeyValuePair<IPokeAByteProperty, string>
                    (instance.Mapper.Properties[path], x.Value?.ToString() ?? string.Empty);
            })
            .ToDictionary();
        //Make sure the addresses are the same
        var baseAddress = gameHookProperties.First().Key.Address;
        var baseType = gameHookProperties.First().Key.Type;
        var baseLength = gameHookProperties.First().Key.Length;
        if (baseAddress is null || baseLength == 0)
            return TypedResults.BadRequest("Address or length for property is null.");
        if (gameHookProperties.Any(x =>
                    x.Key.Address != baseAddress.Value ||
                    x.Key.Type != baseType ||
                    x.Key.Length != baseLength))
            return TypedResults.BadRequest("Addresses or types for the properties are not the same.");

        try
        {
            //Construct the updated byte array from value
            var outputByteArray = MultiplePropertiesUpdater
                .ConstructUpdatedBytes(gameHookProperties, instance.Mapper);
            //Update only the first property since WriteBytes will overwrite the entire address space 
            //of this property. We are maintaining the original set of bytes before overwriting only
            //the values 
            await instance.WriteBytes(gameHookProperties.First().Key, outputByteArray, false);
        }
        catch (Exception e)
        {
            return TypedResults.BadRequest(e.ToString());
        }
        return TypedResults.Ok();
    }

    public static async Task<IResult> UpdatePropertyBytesAsync(IInstanceService instanceService, [FromBody] UpdatePropertyBytesModel model)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        var path = model.Path.StripEndingRoute().FromRouteToPath();
        var actualBytes = Array.ConvertAll(model.Bytes, x => (byte)x);

        var prop = instance.Mapper.Properties[path];

        if (prop == null)
        {
            return TypedResults.NotFound();
        }

        if (prop.IsReadOnly)
        {
            return TypedResults.BadRequest("Property is read only.");
        }

        await instance.WriteBytes(prop, actualBytes, model.Freeze);

        return TypedResults.Ok();
    }
}
