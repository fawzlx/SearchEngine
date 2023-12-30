using System.Text.RegularExpressions;

namespace SearchEngine.Common.Extensions;

public static class HtmlExtension
{
    public static IEnumerable<Uri> ParseUrls(this string content, string originUrl)
    {
        var linkRgx = new Regex("(?<=<a\\s+(?:[^>]*?\\s+)?href=(?:'|\"))[^'\"]*?(?=(?:'|\"))", RegexOptions.Multiline);

        foreach (var matchedLink in linkRgx.Matches(content))
        {
            var link = matchedLink.ToString();
            if (string.IsNullOrWhiteSpace(link))
                continue;

            if (link.StartsWith("/"))
                link = originUrl + link;

            if (!Uri.TryCreate(link, UriKind.Absolute, out var result))
                continue;

            if (!result.AbsoluteUri.Contains(originUrl))
                continue;

            yield return result;
        }
    }

    public static string GetTitle(this string content)
    {
        var title = Regex.Matches(content, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
            RegexOptions.IgnoreCase).FirstOrDefault()?.Groups["Title"].Value;

        if (string.IsNullOrWhiteSpace(title))
            title = Regex.Matches(content, @"\<h1.*(?<=\>)(?!\<)(.*)(?=\<)(?<!\>).*\</h1\>",
                RegexOptions.IgnoreCase).FirstOrDefault()?.Groups[0].Value;

        return string.IsNullOrWhiteSpace(title) ? string.Empty : title;
    }

    public static IEnumerable<string> GetWords(this string content)
    {
        // Replace all style tags with space
        content = Regex.Replace(content, @"<style.*</style>", " ");

        // Replace all script tags with space
        content = Regex.Replace(content, @"<script.*</script>", " ");

        // Replace all tags with a space
        content = Regex.Replace(content, "<(.|\n)+?>", " ");

        // Remove all WhiteSpace and compress all WhiteSpace to one space
        content = Regex.Replace(content, @"\s+", " ");

        content = content
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");

        content = content.NormalizeString();

        return content.Split(' ').Where(x => x.Length > 1 || int.TryParse(x, out _));
    }

    public static IDictionary<string, int> UniqueWords(this IEnumerable<string> words)
    {
        return words.GroupBy(x => x)
            .ToDictionary(x => x.Key, y => y.Count());
    }

    private static string NormalizeString(this string input)
    {
        const string pattern = "[\\~#%&*{}/:;,.،!()<>«»?|\"-]";

        input = Regex.Replace(input, pattern, "");

        input = input
            .Replace("ﮎ", "ک")
            .Replace("ﮏ", "ک")
            .Replace("ﮐ", "ک")
            .Replace("ﮑ", "ک")
            .Replace("ك", "ک")
            .Replace("ي", "ی")
            .Replace("ھ", "ه")
            .Replace(" ", " ")
            .Replace("‌", " ")
            .Replace("|", "")
            .Replace("x200C", "");

        return input;
    }
}