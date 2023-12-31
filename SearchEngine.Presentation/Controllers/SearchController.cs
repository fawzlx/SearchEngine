using Microsoft.AspNetCore.Mvc;
using SearchEngine.Service.Crawl;
using SearchEngine.Service.Search;

namespace SearchEngine.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class SearchController : Controller
{
    private readonly ICrawler _crawler;
    private readonly ISearcher _searcher;
    private readonly IWordDocumentIndex _wordDocumentIndex;

    public SearchController(ICrawler crawler, IWordDocumentIndex wordDocumentIndex, ISearcher searcher)
    {
        _crawler = crawler;
        _wordDocumentIndex = wordDocumentIndex;
        _searcher = searcher;
    }

    [HttpPost("crawl")]
    public async Task<ActionResult> Crawl(CancellationToken cancellationToken)
    {
        await _crawler.Execute(cancellationToken);

        return Ok("success");
    }

    [HttpPost("createIndex")]
    public async Task<ActionResult> InvertedIndex(CancellationToken cancellationToken)
    {
        await _wordDocumentIndex.Execute(cancellationToken);
    
        return Ok("success");
    }

    [HttpGet("search")]
    public ActionResult Search([FromQuery] string query)
    {
        var result = _searcher.SearchQuery(query);
    
        return Ok(result);
    }
}