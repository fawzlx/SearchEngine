using Microsoft.EntityFrameworkCore;
using SearchEngine.Database.Entities;

namespace SearchEngine.Database;

public class SearchEngineDbContext : DbContext
{
    public SearchEngineDbContext(DbContextOptions<SearchEngineDbContext> options) : base(options)
    {
    }

    public DbSet<Page> Pages { get; set; }
    public DbSet<InvertedIndexWordDocument> Words { get; set; }
}