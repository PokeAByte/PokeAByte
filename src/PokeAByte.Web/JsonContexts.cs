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

[JsonSerializable(typeof(MapperUpdaterSettings))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(bool[]))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(List<MapperDto>))]
[JsonSerializable(typeof(List<MapperComparisonDto>))]
[JsonSerializable(typeof(GithubApiSettings))]
[JsonSerializable(typeof(List<ArchivedMapperDto>))]
[JsonSerializable(typeof(IEnumerable<ArchivedMapperDto>))]
[JsonSerializable(typeof(List<MapperFileModel>))]
[JsonSerializable(typeof(IEnumerable<MapperFileModel>))]
[JsonSerializable(typeof(MapperModel))]
[JsonSerializable(typeof(IPokeAByteProperty))]
[JsonSerializable(typeof(List<IPokeAByteProperty>))]
[JsonSerializable(typeof(MapperUpdaterSettings))]
[JsonSerializable(typeof(List<MapperDto>))]
[JsonSerializable(typeof(List<MapperComparisonDto>))]
[JsonSerializable(typeof(IEnumerable<MapperComparisonDto>))]
[JsonSerializable(typeof(List<UpdateMemoryModel>))]
[JsonSerializable(typeof(List<MapperFileModel>))]
[JsonSerializable(typeof(MapperReplaceModel))]
[JsonSerializable(typeof(Dictionary<string, IEnumerable<ArchivedMapperDto>>))]
[JsonSerializable(typeof(UpdatePropertyFreezeModel))]
[JsonSerializable(typeof(UpdatePropertyBytesModel))]
[JsonSerializable(typeof(UpdatePropertyValueModel))]
[JsonSerializable(typeof(List<UpdatePropertyValueModel>))]
[JsonSerializable(typeof(PropertyUpdateModel))]
[JsonSerializable(typeof(ProblemDetails))]
public partial class ApiJsonContext : JsonSerializerContext { }