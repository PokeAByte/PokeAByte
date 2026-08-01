using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Controllers;

using SetPropertyFrozenResult = Results<BadRequest<ProblemDetails>, NotFound, BadRequest<string> , Ok>;
using GetGlossaryResult = Results<BadRequest<ProblemDetails>, Ok<Dictionary<string, IEnumerable<GlossaryItemModel>>>>;
using GetValueResult = Results<BadRequest<ProblemDetails>, NotFound, BadRequest<string>, ContentHttpResult>;
using GetGlossaryPageResult = Results<BadRequest<ProblemDetails>, NotFound, Ok<IEnumerable<GlossaryItemModel>>>;
using GetPropertyResult = Results<NotFound, BadRequest<ProblemDetails>, Ok<IPokeAByteProperty>>;
using GetPropertiesResult = Results<BadRequest<ProblemDetails>, Ok<IEnumerable<IPokeAByteProperty>>>;
using GetMetaResult = Results<BadRequest<ProblemDetails>, Ok<MapperMetaModel>>;
using GetMapperResult = Results<BadRequest<ProblemDetails>, Ok<MapperModel>>;

public static class MapperEndpoints
{
    public static void MapMapperEndpoints(this WebApplication app)
    {
        app.MapGet("/mapper", GetMapper);
        app.MapGet("/mapper/meta", GetMeta);
        app.MapGet("/mapper/properties", GetProperties);
        app.MapGet("/mapper/properties/{**path}/", GetProperty);
        app.MapGet("/mapper/glossary", GetGlossary);
        app.MapGet("/mapper/glossary/{key}", GetGlossaryPage);
        app.MapGet("/mapper/values/{**path}/", GetValueAsync);
        app.MapPost("/mapper/set-property-frozen", SetPropertyFrozenAsync);
        app.MapPost("/mapper/set-property-bytes", SetPropertyBytesAsync);
        app.MapPost("/mapper/set-properties-by-bits", SetPropertiesByBits);
        app.MapPost("/mapper/set-property-value", SetPropertyValueAsync);
    }

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

    public static GetMapperResult GetMapper(IInstanceService instanceService, AppSettingsService appSettingsService)
    {
        if (instanceService.Instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        var model = new MapperModel()
        {
            Meta = MapperMetaModel.FromMapperSection(instanceService.Instance.Mapper.Metadata),
            Properties = instanceService.Instance.Mapper.Properties.Values,
            Glossary = instanceService.Instance.Mapper.References.Values.MapToDictionaryGlossaryItemModel()
        };
        return TypedResults.Ok(model);
    }

    /// <summary>
    /// Get the meta-data of the currently loaded mapper.
    /// </summary>
    /// <param name="instanceService"></param>
    /// <returns>
    /// HTTP 200 With the JSON-serialized mapper metadata. <br/>
    /// HTTP 400 If no mapper is currently loaded.
    /// </returns>
    public static GetMetaResult GetMeta(IInstanceService instanceService)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        var meta = instance.Mapper.Metadata;
        return TypedResults.Ok(MapperMetaModel.FromMapperSection(meta));
    }

    /// <summary>
    /// Get the list of all properties of the currently loaded mapper with their current values.
    /// </summary>
    /// <param name="instanceService"></param>
    /// <returns>
    /// <returns>
    /// HTTP 200 With the JSON-serialized IPokeAByteProperty list. <br/>
    /// HTTP 400 If no mapper is currently loaded.
    /// </returns>
    /// </returns>
    public static GetPropertiesResult GetProperties(IInstanceService instanceService)
    {
        var instance = instanceService.Instance;
        if (instance == null)
        {   
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());
        }

        return TypedResults.Ok((IEnumerable<IPokeAByteProperty>)instance.Mapper.Properties.Values);
    }

    /// <summary>
    /// Get thhe property for the target path.
    /// </summary>
    /// <param name="instanceService"></param>
    /// <param name="path"> The path of the target property.</param>
    /// <returns>
    /// HTTP 200 With the JSON-serialized IPokeAByteProperty. <br/>
    /// HTTP 400 If no mapper is currently loaded. <br/>
    /// HTTP 404 If the target property does not exist.
    /// </returns>
    public static GetPropertyResult GetProperty(IInstanceService instanceService, [FromRoute] string path)
    {
        var instance = instanceService.Instance;
        if (instance == null)
        {   
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());
        }
        path = path.StripEndingRoute();

        if (instance.Mapper.Properties.TryGetValue(path, out var property))
        {
            return TypedResults.Ok(property);
        }
        return TypedResults.NotFound();
    }

    /// <summary>
    /// Get the value of the target property as plain text.
    /// </summary>
    /// <param name="instanceService"></param>
    /// <param name="path"> The path of the property to read. </param>
    /// <returns>
    /// HTTP 200 With the string representation of the value as plain text. <br/>
    /// HTTP 400 If no mapper is currently loaded. <br/>
    /// HTTP 404 If the target property does not exist.
    /// </returns>
    public static GetValueResult GetValueAsync(IInstanceService instanceService, string path)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        path = path.StripEndingRoute();
        if (!instance.Mapper.Properties.TryGetValue(path, out var prop))
        {
            return TypedResults.NotFound();
        }
        if (prop.Value != null && !(prop.Value is string) && !(prop.Value is int))
        {
            return TypedResults.BadRequest($"{prop.Path} is an object and cannot be converted to text.");
        }
        return TypedResults.Text(prop.Value?.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Freezes or unfreezes the value of the target property. Whenever a frozen property changes, Poke-A-Byte will ask 
    /// the emulator to write the frozen value back into the games memory. Some emulators also support freezing the 
    /// respective memory internally.
    /// </summary>
    /// <param name="instanceService"></param>
    /// <param name="request"> The request payload. </param>
    /// <returns>
    /// HTTP 200 if the property could be frozen. <br/>
    /// HTTP 400 If no mapper is currently loaded. <br/>
    /// HTTP 404 if the target property does not exist. <br/>
    /// HTTP 501 if an error occured during the freezing or if the target property is read-only.
    /// </returns>
    public static async Task<SetPropertyFrozenResult> SetPropertyFrozenAsync(
        IInstanceService instanceService,
        [FromBody] SetPropertyFrozenRequest request)
    {
        var instance = instanceService.Instance;
        if (instance == null)
        {   
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());
        }

        var prop = instance.Mapper.Properties[request.Path.StripEndingRoute()];

        if (prop == null)
        {
            return TypedResults.NotFound();
        }

        if (prop.IsReadOnly)
        {
            return TypedResults.BadRequest("Property is read only.");
        }

        if (request.Freeze)
        {
            await instance.FreezeProperty(prop, prop.Bytes);
        }
        else
        {
            await instance.UnfreezeProperty(prop);
        }
        return TypedResults.Ok();
    }

    /// <summary>
    /// Returns the glossary of the current mapper. The glossary is the collection of refereences and their reference
    /// values.
    /// </summary>
    /// <param name="instanceService"></param>
    /// <returns>
    /// - HTTP 200 with the JSON glossary data.
    /// - HTTP 400 if no mapper is currently loaded.
    /// </returns>
    public static GetGlossaryResult GetGlossary(IInstanceService instanceService)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        return TypedResults.Ok(instance.Mapper.References.Values.MapToDictionaryGlossaryItemModel());
    }

    /// <summary>
    /// Returns the list of reference items for the target glossary entry (aka "reference" on IPokeAByteProperty). 
    /// </summary>
    /// <param name="instanceService"></param>
    /// <returns>
    /// - HTTP 200 with the JSON glossary data.
    /// - HTTP 400 if no mapper is currently loaded.
    /// - HTTP 403 if the glossary has no entry for the target key.
    /// </returns>
    public static GetGlossaryPageResult GetGlossaryPage(IInstanceService instanceService, [FromRoute] string key)
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

    /// <summary>
    /// Writes a new value to the target property. Poke-A-Byte will also ask the emulator to update the respective 
    /// memory in the game.
    /// </summary>
    /// <param name="instanceService"></param>
    /// <param name="request"> The request data. </param>
    /// <returns>
    /// HTTP 200 if the property was succesfully updated. <br/>
    /// HTTP 403 if the target property is read-only. <br/>
    /// HTTP 404 if the target property does not exist. <br/>
    /// HTTP 501 if an error occured during the update.
    /// </returns>
    public static async Task<IResult> SetPropertyValueAsync(IInstanceService instanceService, [FromBody] SetPropertyValueRequest request)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        var path = request.Path.StripEndingRoute();
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

        await instance.WriteValue(prop, request.Value, request.Freeze);

        return TypedResults.Ok();
    }

    /// <summary>
    /// Update the value of multiple properties that reference the same game memory at the same time, as setting them 
    /// individually one after the other may cause individual bits to be reset by the later request. <br/>
    /// All properties to update must have the same address and must have a non-null non-empty value to be set.
    /// </summary>
    /// <param name="instanceService"></param>
    /// <param name="request"> The property updates to perform. </param>
    /// <returns>
    /// HTTP 200 if all properties were succesfully updated. <br/>
    /// HTTP 400 If no mapper is currently laoded. <br/>
    /// HTTP 400 If no properties were specified. <br/>
    /// HTTP 400 If one of the property updates has an invalid value. <br/>
    /// HTTP 400 If one of the property updates targets the wrong address. <br/>
    /// HTTP 403 if the target property is read-only. <br/>
    /// HTTP 404 if the target property does not exist. <br/>
    /// HTTP 501 if an error occured during the update.
    /// </returns>
    public static async Task<IResult> SetPropertiesByBits(
        IInstanceService instanceService, 
        List<SetPropertyValueRequest> request)
    {
        var instance = instanceService.Instance;
        if (request.Count == 0)
        {   
            return TypedResults.BadRequest("Properties count is zero.");
        }

        var updates = request.Select(x =>
            {
                var path = x.Path.StripEndingRoute().FromRouteToPath();
                return new KeyValuePair<string, string>(path, x.Value?.ToString() ?? string.Empty);
            });

        WriteMultipleResult? result = instance is null ? null : await instance.WriteMultiple(updates);
        return result switch
        {
            null => TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem()),
            WriteMultipleResult.InvalidPaths => TypedResults.BadRequest("One or more properties do not exist"),
            WriteMultipleResult.InvalidValue => TypedResults.BadRequest("Values cannot be null."),
            WriteMultipleResult.InvalidLengthOrAddress => TypedResults.BadRequest("Address or length for property is null."),
            WriteMultipleResult.InvalidProperty => TypedResults.BadRequest("Addresses or types for the properties are not the same."),
            WriteMultipleResult.Success => TypedResults.Ok(),
            _ => TypedResults.InternalServerError(),
        };
    }

    /// <summary>
    /// Writes a new value to the target property by setting the underlying bytes directly and calculating a new value 
    /// from there. Poke-A-Byte will also ask the emulator to update the respective memory in the game.
    /// </summary>
    /// <param name="instanceService"></param>
    /// <param name="model"> The request data. </param>
    /// <returns>
    /// HTTP 200 if updating the property value via it's underlying bytes was successful. <br/>
    /// HTTP 403 if the target property is read-only. <br/>
    /// HTTP 404 if the target property does not exist. <br/>
    /// HTTP 501 if an error occured during the update.
    /// </returns>
    public static async Task<IResult> SetPropertyBytesAsync(IInstanceService instanceService, [FromBody] SetPropertyBytesRequest model)
    {
        var instance = instanceService.Instance;
        if (instance == null)
            return TypedResults.BadRequest(ApiHelper.MapperNotLoadedProblem());

        var path = model.Path.StripEndingRoute().FromRouteToPath();
        var actualBytes = Array.ConvertAll(model.Bytes, x => (byte)x);
        var property = instance.Mapper.Properties[path];

        if (property == null)
        {
            return TypedResults.NotFound();
        }

        if (property.IsReadOnly)
        {
            return TypedResults.BadRequest("Property is read only.");
        }

        await instance.WriteBytes(property, actualBytes, model.Freeze);

        return TypedResults.Ok();
    }
}
