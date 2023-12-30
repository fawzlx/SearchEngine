using SearchEngine.Database;
using SearchEngine.Service.Fetcher;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NCrontab;
using SearchEngine.Common.Extensions;
using SearchEngine.Database.Entities;

namespace SearchEngine.Service.Crawl;

public class Crawler : ICrawler
{
    private readonly IFetcher _fetcher;
    private readonly SearchEngineDbContext _dbContext;

    private readonly Queue<Uri> _urls;
    private readonly HashSet<Uri> _visitedUrls;

    private readonly string _originUrl;
    private readonly CrontabSchedule _schedule;
    private DateTime _nextRun;
    private int _crawlRemaining;

    public Crawler(IFetcher fetcher, SearchEngineDbContext dbContext, IConfiguration configuration)
    {
        _fetcher = fetcher;
        _dbContext = dbContext;

        _originUrl = configuration.GetSection("SearchEngineSettings")["OriginUrl"]!;
        _crawlRemaining = int.Parse(configuration.GetSection("SearchEngineSettings")["CrawlNumbers"]!);

        var schedule = configuration.GetSection("SearchEngineSettings")["Schedule"]!;
        _schedule = CrontabSchedule.Parse(schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
        _nextRun = _schedule.GetNextOccurrence(DateTime.Now);

        _urls = new Queue<Uri>();
        _visitedUrls = new HashSet<Uri>();
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(_originUrl, UriKind.Absolute, out var originUri))
            throw new ValidationException();

        _urls.Enqueue(originUri);

        do
        {
            if (DateTime.Now <= _nextRun)
                continue;

            await Crawl(cancellationToken);

            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        } while (!cancellationToken.IsCancellationRequested && _crawlRemaining > 0);
    }
    
    #region Private Methods

    private async Task Crawl(CancellationToken cancellationToken)
    {
        var (uri, content) = await Fetch(cancellationToken);
        if (content is null)
            return;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _crawlRemaining--;

        _visitedUrls.Add(uri);

        var contentUrls = content.ParseUrls(_originUrl).ToList();
        
        AddQueue(contentUrls.ToList());
    }

    private void AddQueue(IEnumerable<Uri> contentUrls)
    {
        foreach (var contentUrl in contentUrls)
        {
            if (_visitedUrls.Contains(contentUrl) ||
                _urls.Contains(contentUrl))
                continue;

            _urls.Enqueue(contentUrl);
        }
    }

    private async Task<(Uri url, string? content)> Fetch(CancellationToken cancellationToken)
    {
        var uri = _urls.Dequeue();

        var content = await _fetcher.GetContent(uri, cancellationToken);

        if (!string.IsNullOrEmpty(content))
            await _dbContext.Pages.AddAsync(new Page(uri.ToString(), content), cancellationToken);

        return (uri, content);
    }

    #endregion
}