namespace SearchEngine.Service.Crawl;

public interface ICrawler
{
    Task Execute(CancellationToken cancellationToken = default);
}