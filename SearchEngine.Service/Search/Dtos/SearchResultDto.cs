namespace SearchEngine.Service.Search.Dtos;

public class SearchResultDto
{
    public SearchResultDto(string correctedQuery, IList<PageDto> pages, long queryMilSec)
    {
        CorrectedQuery = correctedQuery;
        Pages = pages;
        QueryMilSec = queryMilSec;
    }

    public string CorrectedQuery { get; set; }
    public IList<PageDto> Pages { get; set; }
    public long QueryMilSec { get; set; }
    public short Count => (short)Pages.Count;
}