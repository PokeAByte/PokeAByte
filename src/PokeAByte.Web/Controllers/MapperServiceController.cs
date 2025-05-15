using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Services.MapperFile;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Controllers;

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[Route("mapper-service")]
public static class MapperServiceEndpoints
{
    public static void MapMapperServiceEndpoints(this WebApplication app)
    {
        app.MapGet("mapper-service/get-mappers", GetMappers);
        app.MapGet("mapper-service/is-connected", GetIsConnected);
        app.MapPut("mapper-service/change-mapper", ChangeMapperAsync);
        app.MapGet("mapper-service/get-metadata", GetMetadata);
        app.MapGet("mapper-service/get-get-properties", GetProperties);
        app.MapGet("mapper-service/get-glossary", GetGlossaryByReferenceKey);
        app.MapPut("mapper-service/write-property", WritePropertyAsync);
        app.MapPut("mapper-service/unload-mapper", UnloadMapperAsync);
    }

    public static IEnumerable<MapperFileModel> GetMappers(MapperFileService mapperFileService)
    {
        return mapperFileService
            .ListInstalled()
            .Select(x => new MapperFileModel(x.Id, x.DisplayName));
    }

    public static bool GetIsConnected(MapperClientService mapperClientService)
    {
        return mapperClientService.IsCurrentlyConnected;
    }

    public static async Task<IResult> ChangeMapperAsync(MapperClientService mapperClientService, [FromBody] string mapperId)
    {
        var mapperResult = await mapperClientService.ChangeMapper(mapperId);
        if (mapperResult.IsSuccess)
        {
            return TypedResults.Ok();
        }
        return TypedResults.NotFound();        
    }

    public static IResult GetMetadata(MapperClientService mapperClientService)
    {
        var metadataResult = mapperClientService.GetMetaData();
        if (metadataResult.IsSuccess)
        {
            return TypedResults.Ok(metadataResult.ResultValue);
        }

        if (metadataResult.IsException)
            return TypedResults.BadRequest(metadataResult.ToString());
        return TypedResults.NotFound(metadataResult.ToString());
    }

    public static IResult GetProperties(IInstanceService instanceService)
    {
        if (instanceService.Instance?.Mapper == null || instanceService.Instance.Mapper.Properties.Count == 0)
        {
            return TypedResults.NotFound();
        }
        var properties = instanceService.Instance.Mapper.Properties.Values;
        return TypedResults.Ok(properties);
    }

    public static IResult GetGlossaryByReferenceKey(MapperClientService mapperClientService, [FromQuery] string glossaryKey)
    {
        var glossaryResult = mapperClientService.GetGlossaryByReferenceKey(glossaryKey);
        if (glossaryResult.IsSuccess)
        {
            return TypedResults.Ok(glossaryResult.ResultValue);
        }

        if (glossaryResult.IsException)
        {
            return TypedResults.BadRequest(glossaryResult.ToString());
        }
        return TypedResults.NotFound(glossaryResult.ToString());
    }

    public static async Task<IResult> WritePropertyAsync(MapperClientService mapperClientService, [FromBody] PropertyUpdateModel model)
    {
        var writeResult = await mapperClientService
            .WritePropertyData(model.Path, model.Value, model.IsFrozen);
        if (writeResult.IsSuccess)
        {
            return TypedResults.Ok();
        }

        if (writeResult.IsException)
        {
            return TypedResults.BadRequest(writeResult.ToString());
        }
        return TypedResults.NotFound(writeResult.ToString());
    }

    [HttpPut]
    [Route("unload-mapper")]
    public static Task UnloadMapperAsync(MapperClientService mapperClientService)
        => mapperClientService.UnloadMapper();
}

public record PropertyUpdateModel(string Path, string Value, bool IsFrozen);
