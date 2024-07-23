﻿namespace PokeAByte.Web.Models;

public record MapperSettingsModel
{
    public required Guid MapperGuid { get; set; }
    public required string MapperName { get; set; }
    public required List<PropertySettingsModel> Properties { get; set; }
}