namespace SearchEngine.Service.Crawl.Dtos;

public class WordDocumentScoreDto
{
    public WordDocumentScoreDto(int wordsTitle, int wordsBody, NormalizedPageDto page, string word)
    {
        WordsTitle = wordsTitle;
        PageId = page.Id;
        WordsBody = wordsBody;
        Title = page.Title;
        Url = page.Url;
        Summary = page.MakeSummary(word);
    }

    public WordDocumentScoreDto(string summary, string url, int pageId, int wordsTitle, int wordsBody, string title)
    {
        Summary = summary;
        Url = url;
        PageId = pageId;
        WordsTitle = wordsTitle;
        WordsBody = wordsBody;
        Title = title;
    }

    public WordDocumentScoreDto()
    {
    }

    public string Summary { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }

    public int PageId { get; set; }
    public int WordsTitle { get; set; }
    public int WordsBody { get; set; }
    public int Score => WordsTitle * 200 + WordsBody * 1;
}