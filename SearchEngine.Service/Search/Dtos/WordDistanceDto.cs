using SearchEngine.Common.Extensions;

namespace SearchEngine.Service.Search.Dtos;

public class WordDistanceDto : WordDto
{
    public WordDistanceDto(string name, string target) : base(name)
    {
        Name = name;
        Target = target;
    }

    public string Target { get; set; }
    public int Distance => Name.ToCompare(Target);

    public int ScoreDistance => Math.Abs(Score - Target.Sum(y => y));
}