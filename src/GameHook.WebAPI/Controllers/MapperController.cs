using GameHook.Application;
using GameHook.Domain;
using GameHook.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace GameHook.WebAPI.Controllers
{
    static class MapperHelper
    {
        public static PropertyModel MapToPropertyModel(this IGameHookProperty x) =>
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

    public record MapperModel
    {
        public MapperMetaModel Meta { get; init; } = null!;
        public IEnumerable<PropertyModel> Properties { get; init; } = null!;
        public Dictionary<string, IEnumerable<GlossaryItemModel>> Glossary { get; init; } = null!;
    }

    public record MapperMetaModel
    {
        public Guid Id { get; init; }
        public string GameName { get; init; } = string.Empty;
        public string GamePlatform { get; init; } = string.Empty;
        public string MapperReleaseVersion { get; init; } = string.Empty;
    }

    public class GlossaryItemModel
    {
        public ulong Key { get; init; }
        public object? Value { get; init; }
    }

    public record MapperReplaceModel(string Id, string Driver);

    public class PropertyModel
    {
        public string Path { get; init; } = string.Empty;

        public string Type { get; init; } = string.Empty;

        public string? MemoryContainer { get; init; } = string.Empty;

        public uint? Address { get; init; }

        public int? Length { get; init; }

        public int? Size { get; init; }

        public string? Reference { get; init; }

        public string? Bits { get; init; }

        public string? Description { get; init; }


        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public object? Value { get; init; }

        public IEnumerable<int>? Bytes { get; init; } = Enumerable.Empty<int>();

        public bool? IsFrozen { get; init; }

        public bool IsReadOnly { get; init; }
    }

    public class UpdatePropertyValueModel
    {
        public string Path { get; init; } = string.Empty;
        public object? Value { get; init; }
        public bool? Freeze { get; init; }
    }

    public class UpdatePropertyBytesModel
    {
        public string Path { get; init; } = string.Empty;
        public int[] Bytes { get; init; } = Array.Empty<int>();
        public bool? Freeze { get; init; }
    }

    public class UpdatePropertyFreezeModel
    {
        public string Path { get; init; } = string.Empty;
        public bool Freeze { get; init; }
    }

    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("mapper")]
    public class MapperController : ControllerBase
    {
        private readonly ILogger<MapperController> _logger;
        public GameHookInstance Instance { get; }
        private readonly AppSettings _appSettings;
        public readonly IBizhawkMemoryMapDriver _bizhawkMemoryMapDriver;
        public readonly IRetroArchUdpPollingDriver _retroArchUdpPollingDriver;
        public readonly IStaticMemoryDriver _staticMemoryDriver;

        public MapperController(
            ILogger<MapperController> logger,
            GameHookInstance gameHookInstance,
            AppSettings appSettings,
            IBizhawkMemoryMapDriver bizhawkMemoryMapDriver,
            IRetroArchUdpPollingDriver retroArchUdpPollingDriver,
            IStaticMemoryDriver nullDriver)
        {
            _logger = logger;

            Instance = gameHookInstance;

            _appSettings = appSettings;
            _bizhawkMemoryMapDriver = bizhawkMemoryMapDriver;
            _retroArchUdpPollingDriver = retroArchUdpPollingDriver;
            _staticMemoryDriver = nullDriver;
        }

        [HttpGet]
        [SwaggerOperation("Returns the mapper that was loaded, with all properties (populated with data).")]
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
        [SwaggerOperation("Changes the active mapper.")]
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
        [SwaggerOperation("Returns the meta section of the mapper file.")]
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
        [SwaggerOperation("Returns a specific property's value by it's path.")]
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
        [SwaggerOperation("Returns all properties loaded from the mapper.")]
        public ActionResult<IEnumerable<PropertyModel>> GetProperties()
        {
            if (Instance.Initalized == false || Instance.Mapper == null)
                return ApiHelper.MapperNotLoaded();

            return Ok(Instance.Mapper.Properties.Values.Select(x => x.MapToPropertyModel()));
        }

        [HttpGet("properties/{**path}/")]
        [SwaggerOperation("Returns a specific property by it's path.")]
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
        [SwaggerOperation("Updates a property's value.")]
        public async Task<ActionResult> UpdatePropertyValueAsync(UpdatePropertyValueModel model)
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

            await prop.WriteValue(model.Value?.ToString() ?? string.Empty, model.Freeze);

            return Ok();
        }

        [HttpPost("set-property-bytes")]
        [SwaggerOperation("Updates a property's bytes.")]
        public async Task<ActionResult> UpdatePropertyBytesAsync(UpdatePropertyBytesModel model)
        {
            if (Instance.Initalized == false || Instance.Mapper == null)
                return ApiHelper.MapperNotLoaded();

            var path = model.Path.StripEndingRoute().FromRouteToPath();
            var actualBytes = model.Bytes.Select(x => (byte)x).ToArray();

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
        [SwaggerOperation("Updates a property's frozen status.")]
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
        [SwaggerOperation("Returns the glossary section of the mapper file.")]
        public ActionResult<Dictionary<string, Dictionary<string, GlossaryItemModel>>> GetGlossary()
        {
            if (Instance.Initalized == false || Instance.Mapper == null)
                return ApiHelper.MapperNotLoaded();

            return Ok(Instance.Mapper.References.Values.MapToDictionaryGlossaryItemModel());
        }

        [HttpGet("glossary/{key}")]
        [SwaggerOperation("Returns a specific glossary by it's key.")]
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
                return Ok(glossaryItem.Values.Select(x => new GlossaryItemModel()
                {
                    Key = x.Key,
                    Value = x.Value
                }));
            }
        }
    }
}