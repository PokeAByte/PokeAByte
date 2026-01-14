using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Models;

public class MapperProblem : IProblemDetails
{
    public MapperProblem(string title, string detail)
    {
        Title = title;
        Detail = detail;
    }

    public string Title { get; }
    public string Detail { get; }
}
