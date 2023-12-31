using SearchEngine.Service.Search.Dtos;

namespace SearchEngine.Service.Search;

public interface ISearcher
{
    Task<SearchResultDto> SearchQuery(string query);
}