﻿using PokeAByte.Domain.Models.Mappers;

namespace PokeAByte.Domain;

public record ArchivedMapperDto
{
    public required string PathDisplayName { get; init; }
    public required string FullPath { get; init; }
    public required MapperDto Mapper { get; init; }
}