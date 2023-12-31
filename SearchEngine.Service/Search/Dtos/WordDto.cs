namespace SearchEngine.Service.Search.Dtos;

public class WordDto
{
    public WordDto(string name)
    {
        Name = name;
    }
    
    public string Name { get; set; }
    public int Score => Name.Sum(x => x);
}