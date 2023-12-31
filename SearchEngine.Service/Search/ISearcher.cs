using SearchEngine.Service.Search.Dtos;

namespace SearchEngine.Service.Search;

public interface ISearcher
{
    SearchResultDto SearchQuery(string query);
}