using GameHook.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GameHook.WebAPI.Controllers
{
    public class MapperFileModel
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("files")]
    public class FilesController(IMapperFilesystemProvider mapperFilesystemProvider) : ControllerBase
    {
        public IMapperFilesystemProvider MapperFilesystemProvider { get; } = mapperFilesystemProvider;

        [SwaggerOperation("Returns a list of all mapper files available inside of the /mappers folder.")]
        [HttpGet("mappers")]
        public ActionResult<IEnumerable<MapperFileModel>> GetMapperFiles()
        {
            MapperFilesystemProvider.CacheMapperFiles();

            return Ok(MapperFilesystemProvider.MapperFiles.Select(x => new MapperFileModel()
            {
                Id = x.Id,
                DisplayName = x.DisplayName
            }));
        }
    }
}