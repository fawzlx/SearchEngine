namespace SearchEngine.Service.Fetcher;

public interface IFetcher
{
    Task<string?> GetContent(Uri uri, CancellationToken cancellationToken = default);
}