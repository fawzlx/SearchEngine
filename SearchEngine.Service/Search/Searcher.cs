using Microsoft.EntityFrameworkCore;
using SearchEngine.Database;
using SearchEngine.Service.Crawl.Dtos;
using SearchEngine.Service.Search.Dtos;

namespace SearchEngine.Service.Search;

public class Searcher : ISearcher
{
    private readonly SearchEngineDbContext _dbContext;

    private readonly IDictionary<string, IList<WordDocumentScoreDto>> _wordDocuments;
    private readonly HashSet<WordDto> _words;

    public Searcher(SearchEngineDbContext dbContext)
    {
        _dbContext = dbContext;

        _wordDocuments = _dbContext.Words.AsNoTracking()
            .Select(x => new InvertedIndexWordDocumentDto(x))
            .ToDictionary(x => x.Key, y => y.Documents);
        _words = _wordDocuments.Select(x => new WordDto(x.Key)).ToHashSet();
    }

    public async Task<IEnumerable<SearchResultDto>> SearchQuery(string query)
    {
        var queryWords = query.Split(' ').Where(x => x.Length > 1);
        queryWords = CorrectMissWords(queryWords);

        if (!queryWords.Any())
            return new List<SearchResultDto>();

        return new List<SearchResultDto>();
    }

    private IEnumerable<string> CorrectMissWords(IEnumerable<string> queries)
    {
        var result = new List<string>();

        foreach (var query in queries)
        {
            var word = _words
                .Where(x => x.Score - query.Sum(y => y) <= 165)
                .Select(x => new WordDistanceDto(x.Name, query)).MinBy(x => x.Score);

            if (word is null)
                return new List<string>();

            result.Add(word.Name);
        }

        return result;
    }
}