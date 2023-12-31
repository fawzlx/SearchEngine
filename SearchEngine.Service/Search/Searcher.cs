using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Common.Extensions;
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

    public async Task<SearchResultDto> SearchQuery(string query)
    {
        query = query.NormalizeString();

        var queryWords = query.GetWords().ToList();

        var correctedQueryWords = CorrectMissWords(queryWords).ToList();

        var searchTime = new Stopwatch();
        searchTime.Start();

        var wordDocuments = SearchPages(correctedQueryWords);

        searchTime.Stop();

        var correctedQuery = string.Join(' ', correctedQueryWords);

        correctedQuery = correctedQuery.Equals(query, StringComparison.InvariantCultureIgnoreCase)
            ? string.Empty
            : correctedQuery;

        return new SearchResultDto(correctedQuery, wordDocuments, searchTime.ElapsedMilliseconds);
    }

    public IList<PageDto> SearchPages(IList<string> queries)
    {
        IEnumerable<WordDocumentScoreDto> result = null;

        foreach (var query in queries)
        {
            if (result is null)
            {
                result = _wordDocuments[query];
                continue;
            }

            result = result.Join(_wordDocuments[query], a => a.PageId, b => b.PageId,
                    (a, b) =>
                        new WordDocumentScoreDto(string.Join("...", a.Summary, b.Summary), a.Url, a.PageId,
                            a.WordsTitle + b.WordsTitle,
                            a.WordsBody + b.WordsBody,
                            a.Title))
                .ToList();
        }

        return result?.OrderByDescending(x => x.Score)
            .Select(x => new PageDto(x.Summary, x.Url, x.Title)).ToList() ?? new List<PageDto>();
    }

    private IEnumerable<string> CorrectMissWords(IEnumerable<string> queries)
    {
        foreach (var query in queries)
        {
            if (_wordDocuments.ContainsKey(query))
            {
                yield return query;
                continue;
            }

            var word = _words
                .Where(x => Math.Abs(x.Score - query.Sum(y => y)) <= 2000)
                .Select(x => new WordDistanceDto(x.Name, query))
                .Where(x => x.Distance < 4)
                .OrderBy(x => x.Distance)
                .ThenBy(x => x.ScoreDistance)
                .FirstOrDefault();

            if (word is null)
                break;

            yield return word.Name;
        }
    }
}