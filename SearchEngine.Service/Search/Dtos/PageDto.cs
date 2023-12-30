namespace SearchEngine.Service.Search.Dtos;

public class PageDto
{
    public PageDto(string summary, string url, string title)
    {
        Summary = summary;
        Url = url;
        Title = title;
    }

    public string Url { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
}