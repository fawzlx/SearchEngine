using SearchEngine.Common.Extensions;

namespace SearchEngine.Service.Search.Dtos;

public class WordDistanceDto
{
    public WordDistanceDto(string name, string target)
    {
        Name = name;
        Target = target;
    }

    public string Name { get; set; }
    public string Target { get; set; }
    public int Score => Name.ToCompare(Target);
}