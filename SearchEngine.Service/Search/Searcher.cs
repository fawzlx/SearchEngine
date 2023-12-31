using Microsoft.EntityFrameworkCore;
using SearchEngine.Common.Extensions;
using SearchEngine.Database;
using SearchEngine.Service.Crawl.Dtos;
using SearchEngine.Service.Search.Dtos;
using System.Diagnostics;

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

    public SearchResultDto SearchQuery(string query)
    {
        var searchTime = new Stopwatch();
        searchTime.Start();

        query = query.NormalizeString();

        var queryWords = query.GetWords().ToList();

        var correctedQueryWords = CorrectMissWords(queryWords).ToList();

        var wordDocuments = SearchPagesRepeated(queryWords, ref correctedQueryWords);

        searchTime.Stop();

        var correctedQuery = string.Join(' ', correctedQueryWords);

        correctedQuery = correctedQuery.Equals(query, StringComparison.InvariantCultureIgnoreCase)
            ? string.Empty
            : correctedQuery;

        return new SearchResultDto(correctedQuery, wordDocuments, query, searchTime.ElapsedMilliseconds);
    }

    private IList<PageDto> SearchPagesRepeated(IReadOnlyList<string> queryWords, ref List<string> correctedQueryWords)
    {
        IList<PageDto> wordDocuments = new List<PageDto>();

        for (var i = 0; i < correctedQueryWords.Count; i++)
        {
            wordDocuments = SearchPages(correctedQueryWords);

            if (wordDocuments.Any())
                break;

            for (var j = 0; j < 3; j++)
            {
                var newQuery = NewQuery(correctedQueryWords, i, queryWords[i], j);

                wordDocuments = SearchPages(newQuery);

                if (!wordDocuments.Any())
                    continue;

                correctedQueryWords = newQuery.ToList();

                return wordDocuments; ;
            }
        }

        return wordDocuments;
    }

    public IList<PageDto> SearchPages(IList<string> queries)
    {
        IEnumerable<WordDocumentScoreDto> result = null;

        foreach (var query in queries.OrderBy(_ => Guid.NewGuid()).ToList())
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

            if (!result.Any())
                return new List<PageDto>();
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

            var a = _words
                .Where(x => Math.Abs(x.Score - query.Sum(y => y)) <= 2000)
                .Select(x => new WordDistanceDto(x.Name, query))
                .Where(x => x.Distance < 4)
                .OrderBy(x => x.Distance)
                .ThenBy(x => x.ScoreDistance).ToList();

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

    private IList<string> NewQuery(IEnumerable<string> queries, int removeIndex, string query, int skip = 0)
    {
        var result = new List<string>(queries)
        {
            [removeIndex] = NearestWord(query, skip)
        };

        return result;
    }

    private string NearestWord(string word, int skip = 0)
    {
        return _words
            .Where(x => Math.Abs(x.Score - word.Sum(y => y)) <= 2000)
            .Select(x => new WordDistanceDto(x.Name, word))
            .Where(x => x.Distance > 0)
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.ScoreDistance)
            .Skip(skip)
            .First().Name;
    }
}