using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Common.Extensions;
using SearchEngine.Database;
using SearchEngine.Service.Crawl.Dtos;

namespace SearchEngine.Service.Crawl;

public class WordDocumentIndex : IWordDocumentIndex
{
    private readonly SearchEngineDbContext _dboContext;

    private readonly IList<InvertedIndexWordDocumentDto> _wordDocuments;

    public WordDocumentIndex(SearchEngineDbContext dboContext)
    {
        _dboContext = dboContext;

        _wordDocuments = new List<InvertedIndexWordDocumentDto>();
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        var pages = await _dboContext.Pages.AsNoTracking().ToListAsync(cancellationToken);

        var a = new Stopwatch();

        foreach (var page in pages)
        {
            var wordsInTitle = page.Title
                .GetWords()
                .UniqueWords();

            var wordsInContent = page.Content
                .GetWords()
                .UniqueWords();

            foreach (var word in wordsInContent)
                AddWordDocument(word.Key, page.Id, 0, word.Value);

            foreach (var word in wordsInTitle)
                AddWordDocument(word.Key, page.Id, word.Value, 0);
        }

        a.Stop();

        Console.WriteLine(a.ElapsedTicks);

        await _dboContext.Words.AddRangeAsync(_wordDocuments.Select(x => x.ConvertToModel()), cancellationToken);

        await _dboContext.SaveChangesAsync(cancellationToken);
    }

    private void AddWordDocument(string word, int pageId, int wordsInTitle, int wordsInContent)
    {
        var wordDoc = _wordDocuments.FirstOrDefault(x => string.Equals(x.Key, word));

        if (wordDoc is null)
        {
            _wordDocuments.Add(new InvertedIndexWordDocumentDto(0, word,
                new WordDocumentScoreDto(wordsInTitle, pageId, wordsInContent)));
            return;
        }

        wordDoc.AddDocument(wordsInTitle, pageId, wordsInContent);
    }
}