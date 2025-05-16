using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Models.Properties;
using PokeAByte.Domain.Services.MapperFile;
using PokeAByte.Infrastructure.Github;
using PokeAByte.Web.Controllers;

namespace PokeAByte.Web;

[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(bool[]))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(MapperUpdaterSettings))]
[JsonSerializable(typeof(GithubApiSettings))]
[JsonSerializable(typeof(PropertyUpdateModel))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(MapperReplaceModel))]
[JsonSerializable(typeof(UpdatePropertyFreezeModel))]
[JsonSerializable(typeof(UpdatePropertyBytesModel))]
[JsonSerializable(typeof(MapperModel))]
[JsonSerializable(typeof(UpdatePropertyValueModel))]
[JsonSerializable(typeof(List<UpdatePropertyValueModel>))]
[JsonSerializable(typeof(List<MapperDto>))]
[JsonSerializable(typeof(IEnumerable<MapperFileModel>))]
[JsonSerializable(typeof(IEnumerable<MapperFileData>))]
[JsonSerializable(typeof(IPokeAByteProperty), GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(IList<IPokeAByteProperty>), GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(List<MapperComparisonDto>))]
[JsonSerializable(typeof(IEnumerable<MapperComparisonDto>))]
[JsonSerializable(typeof(List<UpdateMemoryModel>))]
[JsonSerializable(typeof(List<ArchivedMapperDto>))]
[JsonSerializable(typeof(IEnumerable<ArchivedMapperDto>))]
[JsonSerializable(typeof(Dictionary<string, IEnumerable<ArchivedMapperDto>>))]
public partial class ApiJsonContext : JsonSerializerContext { }