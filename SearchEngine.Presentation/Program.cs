using Microsoft.EntityFrameworkCore;
using SearchEngine.Database;
using SearchEngine.Service.Crawl;
using SearchEngine.Service.Fetcher;
using SearchEngine.Service.Search;
using SearchEngine.WebFramework.Middleware.Extensions;
using SearchEngine.WebFramework.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<SearchEngineDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDB")));

builder.Services.AddScoped<ICrawler, Crawler>();
builder.Services.AddScoped<IFetcher, Fetcher>();
builder.Services.AddScoped<ISearcher, Searcher>();
builder.Services.AddScoped<IWordDocumentIndex, WordDocumentIndex>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCustomExceptionHandler();

app.UseSwaggerAndUi();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
