using SearchEngine.Service.Search.Dtos;

namespace SearchEngine.Service.Search;

public interface ISearcher
{
    Task<IEnumerable<SearchResultDto>> SearchQuery(string query);
}