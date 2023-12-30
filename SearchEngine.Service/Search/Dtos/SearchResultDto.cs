namespace SearchEngine.Service.Search.Dtos;

public class SearchResultDto
{
    public SearchResultDto(string correctedQuery, IList<PageDto> pages)
    {
        CorrectedQuery = correctedQuery;
        Pages = pages;
    }

    public string CorrectedQuery { get; set; }
    public IList<PageDto> Pages { get; set; }
    public short Count => (short)Pages.Count;
}