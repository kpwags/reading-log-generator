using Notion.Client;
using ReadingLogGenerator.Configuration;
using ReadingLogGenerator.Models;

namespace ReadingLogGenerator.Services;

public class NotionService
{
    private readonly NotionConfiguration _notionConfiguration;

    public NotionService(NotionConfiguration config)
    {
        _notionConfiguration = config;
    }

    public async Task<List<Article>> GetReadingLogArticles(int logNumber)
    {
        var articles = new List<Article>();
        
        var client = NotionClientFactory.Create(new ClientOptions
        {
            AuthToken = _notionConfiguration.NotionApiKey,
        });

        var readingLogFilter = new NumberFilter("Issue", equal: logNumber);
        var queryParams = new DatabasesQueryParameters { Filter = readingLogFilter };
        
        var pages = await client.Databases.QueryAsync(_notionConfiguration.ReadingLogDbId, queryParams);

        foreach (var page in pages.Results)
        {
            var article = new Article();

            var title = page.Properties.FirstOrDefault(p => p.Key.ToLower() == "title");
            var author = page.Properties.FirstOrDefault(p => p.Key.ToLower() == "author");
            var url = page.Properties.FirstOrDefault(p => p.Key.ToLower() == "url");
            var category = page.Properties.FirstOrDefault(p => p.Key.ToLower() == "category");

            var titleValue = title.Value as TitlePropertyValue;
            var authorValue = author.Value as RichTextPropertyValue;
            var urlValue = url.Value as UrlPropertyValue;
            var categoryValue = category.Value as SelectPropertyValue;

            article.Title = titleValue?.Title?.FirstOrDefault()?.PlainText ?? "";
            article.Author = authorValue?.RichText?.FirstOrDefault()?.PlainText ?? "";
            article.Url = urlValue?.Url ?? "";
            article.Category = GetCategoryFromNotionCategory(categoryValue?.Select.Name.ToLower() ?? "");

            articles.Add(article);
        }
        
        return articles;
    }

    private ReadingLogCategory GetCategoryFromNotionCategory(string category) => category switch
    {
        "software development & design" => ReadingLogCategory.DevelopmentDesign,
        "technology & the internet" => ReadingLogCategory.Technology,
        "science" => ReadingLogCategory.Science,
        "gaming" => ReadingLogCategory.Gaming,
        "business & finance" => ReadingLogCategory.Business,
        "sports" => ReadingLogCategory.Sports,
        "podcasts" => ReadingLogCategory.Podcasts,
        "in depth" => ReadingLogCategory.InDepth,
        _ => ReadingLogCategory.Everything,
    };
}