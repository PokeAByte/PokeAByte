using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Web.Models;

namespace PokeAByte.Web;

[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(bool[]))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(AppSettingsDto))]
[JsonSerializable(typeof(DownloadSettings))]
[JsonSerializable(typeof(PropertyUpdateModel))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(MapperReplaceModel))]
[JsonSerializable(typeof(UpdatePropertyFreezeModel))]
[JsonSerializable(typeof(UpdatePropertyBytesModel))]
[JsonSerializable(typeof(MapperModel))]
[JsonSerializable(typeof(UpdatePropertyValueModel))]
[JsonSerializable(typeof(List<UpdatePropertyValueModel>))]
[JsonSerializable(typeof(List<MapperFile>))]
[JsonSerializable(typeof(IEnumerable<InstalledMapper>))]
[JsonSerializable(typeof(IPokeAByteProperty), GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(IProblemDetails), GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(MapperProblem), GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(IList<IPokeAByteProperty>), GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(List<RemoteMapperFile>))]
[JsonSerializable(typeof(IEnumerable<RemoteMapperFile>))]
[JsonSerializable(typeof(List<UpdateMemoryModel>))]
[JsonSerializable(typeof(List<ArchivedMapperFile>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(IEnumerable<ArchivedMapperFile>))]
[JsonSerializable(typeof(Dictionary<string, IEnumerable<ArchivedMapperFile>>))]
public partial class ApiJsonContext : JsonSerializerContext { }