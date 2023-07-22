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

    private static StringBuilder _markdownBuilder = new StringBuilder();

    static async Task Main()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true)
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
        int logNumber;

        if (!int.TryParse(logNumberString, out logNumber))
        {
            Console.WriteLine("Invalid Output");
            return;
        }

        var notionService = new NotionService(_notifonConfiguration);

        var articles = await notionService.GetReadingLogArticles(logNumber);

        var markdown = GetMarkdownString(articles, logNumber);

        var path = Path.Join(_directoriesConfiguration.Output, $"{logNumber}.md");
        
        using (var tw = new StreamWriter(path, true))
        {
            tw.Write(markdown);
        }
    }

    private static string GetMarkdownString(List<Article> articles, int logNumber)
    {
        _markdownBuilder.AppendLine($"# Reading Log - {DateTime.Now.ToString("MMMM d, yyyy")} (#{logNumber})");
        _markdownBuilder.AppendLine("");
        _markdownBuilder.AppendLine("Introduction Text");
        _markdownBuilder.AppendLine("");

        if (articles.Any(a => a.Category == ReadingLogCategory.InDepth))
        {
            _markdownBuilder.AppendLine("## In Depth");
            _markdownBuilder.AppendLine("");

            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.InDepth));
        }

        _markdownBuilder.AppendLine("## Link Blast");
        _markdownBuilder.AppendLine("");
        
        if (articles.Any(a => a.Category == ReadingLogCategory.DevelopmentDesign))
        {
            _markdownBuilder.AppendLine("### 👨🏼‍💻 Software Development & Design");
            _markdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.DevelopmentDesign));
            
            _markdownBuilder.AppendLine("---");
            _markdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Technology))
        {
            _markdownBuilder.AppendLine("### 🖥 Technology & the Internet");
            _markdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.Technology));
            
            _markdownBuilder.AppendLine("---");
            _markdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Science))
        {
            _markdownBuilder.AppendLine("### 🔬 Science");
            _markdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.Science));
            
            _markdownBuilder.AppendLine("---");
            _markdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Gaming))
        {
            _markdownBuilder.AppendLine("### 🎮 Gaming");
            _markdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.Gaming));
            
            _markdownBuilder.AppendLine("---");
            _markdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Business))
        {
            _markdownBuilder.AppendLine("### 📈 Business & Finance");
            _markdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.Business));
            
            _markdownBuilder.AppendLine("---");
            _markdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Sports))
        {
            _markdownBuilder.AppendLine("### ⚾ Sports");
            _markdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.Sports));
            
            _markdownBuilder.AppendLine("---");
            _markdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Podcasts))
        {
            _markdownBuilder.AppendLine("### 🎧 Podcasts");
            _markdownBuilder.AppendLine("");
            
            foreach (var article in articles.Where(a => a.Category == ReadingLogCategory.Podcasts))
            {
                _markdownBuilder.AppendLine($"[{article.Author}: {article.Title}]({article.Url})");
                _markdownBuilder.AppendLine("");
            }
            
            _markdownBuilder.AppendLine("---");
            _markdownBuilder.AppendLine("");
        }
        
        if (articles.Any(a => a.Category == ReadingLogCategory.Everything))
        {
            _markdownBuilder.AppendLine("### 🎒 Everything Else");
            _markdownBuilder.AppendLine("");
            
            AddLinks(articles.Where(a => a.Category == ReadingLogCategory.Everything));
            
            _markdownBuilder.AppendLine("---");
            _markdownBuilder.AppendLine("");
        }

        _markdownBuilder.AppendLine("🎵 A Song to Leave You With");
        _markdownBuilder.AppendLine("");
        _markdownBuilder.AppendLine("#### Artist - Song");
        _markdownBuilder.AppendLine("");


        return _markdownBuilder.ToString();
    }

    private static void AddLinks(IEnumerable<Article> articles)
    {
        foreach (var article in articles)
        {
            _markdownBuilder.AppendLine($"[{article.Title}]({article.Url}) - *{article.Author}*");
            _markdownBuilder.AppendLine("");
        }
    }
}