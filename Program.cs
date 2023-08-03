using System.Text;
using Microsoft.Extensions.Configuration;
using ReadingLogGenerator.Configuration;
using ReadingLogGenerator.Models;
using ReadingLogGenerator.Services;

namespace ReadingLogGenerator;

internal class Program
{
    private static DirectoriesConfiguration? _directoriesConfiguration;
    private static NotionConfiguration? _notifonConfiguration;

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
            Console.WriteLine("Invalid Output");
            return;
        }

        var notionService = new NotionService(_notifonConfiguration);

        var articles = await notionService.GetReadingLogArticles(logNumber);

        var markdown = GetMarkdownString(articles, logNumber);

        var path = Path.Join(_directoriesConfiguration.Output, $"{logNumber}.md");

        await using var tw = new StreamWriter(path, true);
        
        await tw.WriteAsync(markdown);
    }

    private static string GetMarkdownString(List<Article> articles, int logNumber)
    {
        MarkdownBuilder.AppendLine($"# Reading Log - {DateTime.Now.ToString("MMMM d, yyyy")} (#{logNumber})");
        MarkdownBuilder.AppendLine("");
        MarkdownBuilder.AppendLine("Introduction Text");
        MarkdownBuilder.AppendLine("");

        if (articles.Any(a => a.Category == ReadingLogCategory.InDepth))
        {
            MarkdownBuilder.AppendLine("## In Depth");
            MarkdownBuilder.AppendLine("");

            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.InDepth));
        }

        MarkdownBuilder.AppendLine("## Link Blast");
        MarkdownBuilder.AppendLine("");
        
        if (articles.Any(a => a.Category == ReadingLogCategory.DevelopmentDesign))
        {
            MarkdownBuilder.AppendLine("### 👨🏼‍💻 Software Development & Design");
            MarkdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.DevelopmentDesign));
            
            MarkdownBuilder.AppendLine("---");
            MarkdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Technology))
        {
            MarkdownBuilder.AppendLine("### 🖥 Technology & the Internet");
            MarkdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.Technology));
            
            MarkdownBuilder.AppendLine("---");
            MarkdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Science))
        {
            MarkdownBuilder.AppendLine("### 🔬 Science");
            MarkdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.Science));
            
            MarkdownBuilder.AppendLine("---");
            MarkdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Gaming))
        {
            MarkdownBuilder.AppendLine("### 🎮 Gaming");
            MarkdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.Gaming));
            
            MarkdownBuilder.AppendLine("---");
            MarkdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Business))
        {
            MarkdownBuilder.AppendLine("### 📈 Business & Finance");
            MarkdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.Business));
            
            MarkdownBuilder.AppendLine("---");
            MarkdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Sports))
        {
            MarkdownBuilder.AppendLine("### ⚾ Sports");
            MarkdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.Sports));
            
            MarkdownBuilder.AppendLine("---");
            MarkdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Podcasts))
        {
            MarkdownBuilder.AppendLine("### 🎧 Podcasts");
            MarkdownBuilder.AppendLine("");
            
            foreach (var article in articles.Where(a => a.Category == ReadingLogCategory.Podcasts))
            {
                MarkdownBuilder.AppendLine($"[{article.Author}: {article.Title}]({article.Url})");
                MarkdownBuilder.AppendLine("");
            }
            
            MarkdownBuilder.AppendLine("---");
            MarkdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Everything))
        {
            MarkdownBuilder.AppendLine("### 🎒 Everything Else");
            MarkdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.Everything));
            
            MarkdownBuilder.AppendLine("---");
            MarkdownBuilder.AppendLine("");
        }

        MarkdownBuilder.AppendLine("🎵 A Song to Leave You With");
        MarkdownBuilder.AppendLine("");
        MarkdownBuilder.AppendLine("#### Artist - Song");
        MarkdownBuilder.AppendLine("");


        return MarkdownBuilder.ToString();
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