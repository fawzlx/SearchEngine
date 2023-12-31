using Microsoft.EntityFrameworkCore;
using SearchEngine.Common.Extensions;
using SearchEngine.Database;
using SearchEngine.Database.Entities;
using SearchEngine.Service.Crawl.Dtos;
using System.Diagnostics;

namespace SearchEngine.Service.Crawl;

public class WordDocumentIndex : IWordDocumentIndex
{
    private readonly SearchEngineDbContext _dboContext;

    private readonly IDictionary<string, IList<WordDocumentScoreDto>> _wordDocuments;

    public WordDocumentIndex(SearchEngineDbContext dboContext)
    {
        _dboContext = dboContext;

        _wordDocuments = new Dictionary<string, IList<WordDocumentScoreDto>>();
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {

        var a = new Stopwatch();

        a.Start();

        var pages = await _dboContext.Pages.AsNoTracking()
            .Select(x => new NormalizedPageDto(x))
            .ToListAsync(cancellationToken);

        foreach (var page in pages)
        {
            var wordsInTitle = page.Title
                .GetWords()
                .UniqueWords();

            var wordsInContent = page.NormalizeBody
                .GetWords()
                .UniqueWords();

            foreach (var word in wordsInContent)
                AddWordDocument(word.Key, page, 0, word.Value);

            foreach (var word in wordsInTitle)
                AddWordDocument(word.Key, page, word.Value, 0);
        }

        await _dboContext.Words.AddRangeAsync(
            _wordDocuments.Select(x =>
                new InvertedIndexWordDocument(x.Key, x.Value)),
            cancellationToken);

        await _dboContext.SaveChangesAsync(cancellationToken);

        a.Stop();

        Console.WriteLine(a.ElapsedTicks);
    }

    private void AddWordDocument(string word, NormalizedPageDto page, int wordsInTitle, int wordsInContent)
    {
        var wordDocumentScoreDto = new WordDocumentScoreDto(wordsInTitle, wordsInContent, page, word);

        if (_wordDocuments.TryGetValue(word, out var documents))
        {
            var document = documents.FirstOrDefault(x => x.PageId == page.Id);

            if (document is null)
            {
                documents.Add(wordDocumentScoreDto);
                return;
            }

            document.WordsBody += wordsInContent;
            document.WordsTitle += wordsInTitle;
        }
        else
            _wordDocuments.Add(word, new List<WordDocumentScoreDto> { wordDocumentScoreDto });
    }
}