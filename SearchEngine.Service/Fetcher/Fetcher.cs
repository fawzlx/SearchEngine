namespace SearchEngine.Service.Fetcher;

public class Fetcher : IFetcher
{
    public async Task<string?> GetContent(Uri uri, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient();

            var result = await client.GetAsync(uri, cancellationToken);

            if (result is not { IsSuccessStatusCode: true })
                return null;

            if (!result.Content.Headers.ContentType!.MediaType!.Contains("text/html"))
                return null;

            return await result.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception)
        {
            return default;
        }
    }
}