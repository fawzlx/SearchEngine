namespace SearchEngine.Service.Crawl.Dtos;

public class WordDocumentScoreDto
{
    public WordDocumentScoreDto(int wordsTitle, int pageId, int wordsBody)
    {
        WordsTitle = wordsTitle;
        PageId = pageId;
        WordsBody = wordsBody;
    }

    public int PageId { get; set; }
    public int WordsTitle { get; set; }
    public int WordsBody { get; set; }
    public int Score => WordsTitle * 3 + WordsBody * 1;
}