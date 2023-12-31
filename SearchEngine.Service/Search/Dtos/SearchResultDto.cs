namespace SearchEngine.Service.Search.Dtos;

public class SearchResultDto
{
    public SearchResultDto(string correctedQuery, IList<PageDto> pages, string searchQuery, long time)
    {
        CorrectedQuery = correctedQuery;
        Pages = pages;
        SearchQuery = searchQuery;
        Time = time;
    }

    public string SearchQuery { get; set; }

    public string CorrectedQuery { get; set; }
    public IList<PageDto> Pages { get; set; }
    public long Time { get; set; }
    public short Count => (short)Pages.Count;
}