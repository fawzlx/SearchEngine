using Newtonsoft.Json;
using SearchEngine.Database.Entities;

namespace SearchEngine.Service.Crawl.Dtos;

public class InvertedIndexWordDocumentDto
{
    public InvertedIndexWordDocumentDto(InvertedIndexWordDocument document)
    {
        Id = document.Id;
        Key = document.Key;
        Documents = string.IsNullOrWhiteSpace(document.Value)
            ? new List<WordDocumentScoreDto>()
            : JsonConvert.DeserializeObject<IList<WordDocumentScoreDto>>(document.Value)
              ?? new List<WordDocumentScoreDto>();
    }

    public InvertedIndexWordDocumentDto(int id, string key, string value)
    {
        Id = id;
        Key = key;
        Documents = string.IsNullOrWhiteSpace(value)
            ? new List<WordDocumentScoreDto>()
            : JsonConvert.DeserializeObject<IList<WordDocumentScoreDto>>(value)
              ?? new List<WordDocumentScoreDto>();
    }

    public InvertedIndexWordDocumentDto(int id, string key, WordDocumentScoreDto document)
    {
        Id = id;
        Key = key;
        Documents = new List<WordDocumentScoreDto>{ document };
    }

    public int Id { get; set; }

    public string Key { get; set; }

    public IList<WordDocumentScoreDto> Documents { get; set; }

    public InvertedIndexWordDocument ConvertToModel() => new(Key, JsonConvert.SerializeObject(Documents));

    public void AddDocument(int wordsTitle, int pageId, int wordsBody)
    {
        var document = Documents.FirstOrDefault(x => x.PageId == pageId);

        if (document == null)
        {
            Documents.Add(new WordDocumentScoreDto(wordsTitle, pageId, wordsBody));
            return;
        }

        document.WordsBody += wordsBody;
        document.WordsTitle += wordsTitle;
    }
}