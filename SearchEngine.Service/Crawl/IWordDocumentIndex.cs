namespace SearchEngine.Service.Crawl;

public interface IWordDocumentIndex
{
    Task Execute(CancellationToken cancellationToken = default);
}