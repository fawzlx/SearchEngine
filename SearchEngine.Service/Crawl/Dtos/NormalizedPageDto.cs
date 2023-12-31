using System.Linq;
using System.Text;
using SearchEngine.Database.Entities;

namespace SearchEngine.Service.Crawl.Dtos;

public class NormalizedPageDto
{
    public NormalizedPageDto(Page page)
    {
        Id = page.Id;
        Title = page.Title;
        Url = page.Url;
        NormalizeBody = page.NormalizeBody;
    }

    public string MakeSummary(string word)
    {
        var result = new StringBuilder();

        var firstIndex = NormalizeBody.IndexOf(word, StringComparison.OrdinalIgnoreCase);

        if (firstIndex < 0)
            firstIndex = 0;

        var lastWords = NormalizeBody.Substring(0, firstIndex).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (lastWords.Length > 20)
            result.Append(string.Join(' ', lastWords.TakeLast(20)));
        else
            result.Append(string.Join(' ', lastWords));

        var nextWords = NormalizeBody.Substring(firstIndex).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (lastWords.Length < 20)
            result.Append(' ').Append(string.Join(' ', nextWords.Take(40 - lastWords.Length)));
        else if (nextWords.Length <= 20)
            result.Clear().Append(string.Join(' ', lastWords.Take(40 - nextWords.Length))).Append(' ').Append(string.Join(' ', nextWords));
        else if (nextWords.Length > 20)
            result.Append(' ').Append(string.Join(' ', nextWords.Take(40)));

        return result.ToString();
    }

    public int Id { get; set; }

    public string Title { get; set; }

    public string Url { get; set; }

    public string NormalizeBody { get; set; }
}