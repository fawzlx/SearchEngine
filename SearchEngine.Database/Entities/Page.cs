﻿using System.ComponentModel.DataAnnotations;
using SearchEngine.Common.Extensions;

namespace SearchEngine.Database.Entities;

public class Page
{
    public Page(string url, string content)
    {
        Url = url;
        Content = content;
        Title = content.GetTitle();
        CrawlTime = DateTime.Now;
    }

    public Page(string url, string content, DateTime crawlTime)
    {
        Url = url;
        Content = content;
        Title = content.GetTitle();
        CrawlTime = crawlTime;
    }

    [Key]
    public int Id { get; set; }

    [StringLength(150)] public string Title { get; set; }

    [StringLength(300)] public string Url { get; set; }

    public string Content { get; set; }

    public DateTime CrawlTime { get; set; }
}