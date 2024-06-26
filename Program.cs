﻿using System.Text;
using Microsoft.Extensions.Configuration;
using ReadingLogGenerator.Configuration;
using ReadingLogGenerator.Models;
using ReadingLogGenerator.Services;

namespace ReadingLogGenerator;

internal class Program
{
    private static DirectoriesConfiguration? _directoriesConfiguration;
    private static NotionConfiguration? _notifonConfiguration;
    private static List<Article> _articles = new();
    
    private static readonly StringBuilder MarkdownBuilder = new ();

    static async Task Main()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
        
        _directoriesConfiguration = config.GetRequiredSection("Directories").Get<DirectoriesConfiguration>();
        _notifonConfiguration = config.GetRequiredSection("Notion").Get<NotionConfiguration>();

        if (_directoriesConfiguration is null || _notifonConfiguration is null)
        {
            Console.WriteLine("Unable to read settings");
            return;
        }
        
        Console.Write("Please Enter Reading Log Number: ");

        var logNumberString = Console.ReadLine();

        if (!int.TryParse(logNumberString, out int logNumber) || logNumber == 0)
        {
            Console.WriteLine("Invalid input");
            return;
        }

        var notionService = new NotionService(_notifonConfiguration);

        _articles = await notionService.GetReadingLogArticles(logNumber);

        var markdown = GetMarkdownString(logNumber);

        var path = Path.Join(_directoriesConfiguration.Output, $"{logNumber}.md");

        await using var sw = new StreamWriter(path, true);
        
        await sw.WriteAsync(markdown);
    }

    private static string GetMarkdownString(int logNumber)
    {
        var utcDateTime = string.Format("{0:yyyy-MM-ddTHH:mm:ss.FFFZ}", DateTime.UtcNow);
        
        MarkdownBuilder.AppendLine("---");
        MarkdownBuilder.AppendLine($"title: 'Reading Log - {DateTime.Now.ToString("MMMM d, yyyy")} (#{logNumber})'");
        MarkdownBuilder.AppendLine($"date: '{utcDateTime}'");
        MarkdownBuilder.AppendLine($"permalink: /reading-log/{logNumber}/index.html");
        MarkdownBuilder.AppendLine("tags:");
        MarkdownBuilder.AppendLine("  - Reading Log");
        MarkdownBuilder.AppendLine("---");
        
        MarkdownBuilder.AppendLine("");
        MarkdownBuilder.AppendLine("Introduction Text");
        MarkdownBuilder.AppendLine("<!-- excerpt -->");
        MarkdownBuilder.AppendLine("");
        
        AddSection(ReadingLogCategory.DotNet, "🖥 .NET");
        AddSection(ReadingLogCategory.WebDevelopment, "🌐 Web Development");
        AddSection(ReadingLogCategory.Development, "💻 General Development");
        AddSection(ReadingLogCategory.Design, "🎨 Design");
        AddSection(ReadingLogCategory.Internet, "📡 The Internet");
        AddSection(ReadingLogCategory.Technology, "🔌 Technology");
        AddSection(ReadingLogCategory.Science, "🔬 Science");
        AddSection(ReadingLogCategory.Space, "🚀 Space");
        AddSection(ReadingLogCategory.ClimateChange, "🌎 Climate Change");
        AddSection(ReadingLogCategory.Gaming, "🎮 Gaming");
        AddSection(ReadingLogCategory.Business, "📈 Business & Finance");
        AddSection(ReadingLogCategory.Sports, "⚾️ Sports");
        AddSection(ReadingLogCategory.Fitness, "🏃 Health & Fitness");

        if (_articles.Any(a => a.Category == ReadingLogCategory.Podcasts))
        {
            MarkdownBuilder.AppendLine("## 🎧 Podcasts");
            MarkdownBuilder.AppendLine("");
            
            foreach (var article in _articles.Where(a => a.Category == ReadingLogCategory.Podcasts))
            {
                MarkdownBuilder.AppendLine($"[{article.Author}: {article.Title}]({article.Url})");
                MarkdownBuilder.AppendLine("");
            }
            
            MarkdownBuilder.AppendLine("---");
            MarkdownBuilder.AppendLine("");
        }
        
        AddSection(ReadingLogCategory.Entertainment, "📺 Media & Entertainment");
        AddSection(ReadingLogCategory.Longform, "📝 Longform");
        AddSection(ReadingLogCategory.Journalism, "📰 Journalism");
        AddSection(ReadingLogCategory.Politics, "🏛️ Politics");
        AddSection(ReadingLogCategory.Everything, "🎒 Everything Else");
        
        MarkdownBuilder.AppendLine("## 🎵 A Song to Leave You With");
        MarkdownBuilder.AppendLine("");
        
        MarkdownBuilder.AppendLine("<h3 class=\"music\">Artist - Song</h3>");
        MarkdownBuilder.AppendLine("");
        MarkdownBuilder.AppendLine($"{{% youTubeEmbed \"\" \"\" %}}");
        MarkdownBuilder.AppendLine("");

        return MarkdownBuilder.ToString();
    }

    private static void AddSection(ReadingLogCategory category, string title)
    {
        if (_articles.Any(a => a.Category == category))
        {
            MarkdownBuilder.AppendLine($"## {title}");
            MarkdownBuilder.AppendLine("");
            
            AddLinks(_articles.Where(a => a.Category == category));
            
            MarkdownBuilder.AppendLine("---");
            MarkdownBuilder.AppendLine("");
        }
    }

    private static void AddLinks(IEnumerable<Article> articles)
    {
        foreach (var article in articles)
        {
            MarkdownBuilder.AppendLine($"[{article.Title}]({article.Url}) - *{article.Author}*");
            MarkdownBuilder.AppendLine("");
        }
    }
}