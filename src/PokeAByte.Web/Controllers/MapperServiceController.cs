﻿using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Models.Properties;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Controllers;

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[Route("mapper-service")]
public class MapperServiceController : ControllerBase
{
    private readonly ILogger<MapperServiceController> _logger;
    private readonly MapperClientService _mapperClientService;

    public MapperServiceController(ILogger<MapperServiceController> logger, 
        MapperClientService mapperClientService)
    {
        _logger = logger;
        _mapperClientService = mapperClientService;
    }

    [HttpGet]
    [Route("get-mappers")]
    public ActionResult<List<MapperFileModel>> GetMappers()
    {
        return Ok(_mapperClientService.GetMappers().ToList());
    }

    [HttpGet]
    [Route("is-connected")]
    public ActionResult<bool> GetIsConnected()
    {
        return Ok(_mapperClientService.IsCurrentlyConnected);
    }

    [HttpPut]
    [Route("change-mapper")]
    public async Task<IActionResult> ChangeMapperAsync([FromBody] string mapperId)
    {
        try
        {
            var mapperResult = await _mapperClientService.ChangeMapper(
                mapperId, 
                null, 
                null);
            if (mapperResult.IsSuccess)
            {
                return Ok();
            }
            return NotFound();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Bad Request");
            return BadRequest(e);
        }
    }

    [HttpGet]
    [Route("get-metadata")]
    public ActionResult<MapperMetaModel> GetMetadata()
    {
        var metadataResult = _mapperClientService.GetMetaData();
        if (metadataResult.IsSuccess)
        {
            return Ok(metadataResult.ResultValue);
        }

        if (metadataResult.IsException)
            return BadRequest(metadataResult.ToString());
        return NotFound(metadataResult.ToString());
    }

    [HttpGet]
    [Route("get-properties")]
    public ActionResult<List<PropertyModel>> GetProperties()
    {
        var properties = _mapperClientService.Properties;
        if (properties.Count == 0)
        {
            return NotFound();
        }
        return Ok(properties);
    }

    [HttpGet]
    [Route("get-glossary")]
    public ActionResult<List<GlossaryItemModel>> GetGlossaryByReferenceKey([FromQuery] string glossaryKey)
    {
        var glossaryResult = _mapperClientService.GetGlossaryByReferenceKey(glossaryKey);
        if (glossaryResult.IsSuccess)
        {
            return Ok(glossaryResult.ResultValue);
        }

        if (glossaryResult.IsException)
        {
            return BadRequest(glossaryResult.ToString());
        }

        return NotFound(glossaryResult.ToString());
    }

    [HttpPut]
    [Route("write-property")]
    public async Task<IActionResult> WritePropertyAsync([FromBody] PropertyUpdateModel model)
    {
        var writeResult = await _mapperClientService
            .WritePropertyData(model.Path, model.Value, model.IsFrozen);
        if (writeResult.IsSuccess)
        {
            return Ok();
        }

        if (writeResult.IsException)
        {
            return BadRequest(writeResult.ToString());
        }
        return NotFound(writeResult.ToString());
    }

    [HttpPut]
    [Route("unload-mapper")]
    public async Task<IActionResult> UnloadMapperAsync()
    {
        await _mapperClientService.UnloadMapper();
        return Ok();
    }
}

public class PropertyUpdateModel
{
    public string Path { get; set; }
    public string Value { get; set; }
    public bool IsFrozen { get; set; }
}