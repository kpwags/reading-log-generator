using Notion.Client;
using ReadingLogGenerator.Configuration;
using ReadingLogGenerator.Models;

namespace ReadingLogGenerator.Services;

public class NotionService
{
    private readonly NotionConfiguration _notionConfiguration;
    private readonly NotionClient _notionClient;

    public NotionService(NotionConfiguration config)
    {
        _notionConfiguration = config;
        _notionClient = NotionClientFactory.Create(new ClientOptions
        {
            AuthToken = _notionConfiguration.NotionApiKey,
        });
    }

    public async Task<List<Article>> GetReadingLogArticles(int logNumber)
    {
        var articles = new List<Article>();

        bool hasMore;
        string? cursor = null;

        do
        {
            var result = await FetchFromNotion(logNumber, cursor);

            articles.AddRange(MapNotionResultsToArticles(result.Results));

            hasMore = result.HasMore;
            cursor = result.NextCursor;
        } while (hasMore);
        
        return articles;
    }
    
    private async Task<PaginatedList<Page>> FetchFromNotion(int logNumber, string? cursor)
    {
        var readingLogFilter = new NumberFilter("Issue", equal: logNumber);
        var queryParams = new DatabasesQueryParameters { Filter = readingLogFilter, StartCursor = cursor };
        
        var pages = await _notionClient.Databases.QueryAsync(_notionConfiguration.ReadingLogDbId, queryParams);

        return pages;
    }

    private List<Article> MapNotionResultsToArticles(List<Page> results)
    {
        var articles = new List<Article>();
        
        foreach (var page in results)
        {
            var title = page.Properties.FirstOrDefault(p => p.Key.ToLower() == "title");
            var author = page.Properties.FirstOrDefault(p => p.Key.ToLower() == "author");
            var url = page.Properties.FirstOrDefault(p => p.Key.ToLower() == "url");
            var category = page.Properties.FirstOrDefault(p => p.Key.ToLower() == "category");

            var titleValue = title.Value as TitlePropertyValue;
            var authorValue = author.Value as RichTextPropertyValue;
            var urlValue = url.Value as UrlPropertyValue;
            var categoryValue = category.Value as SelectPropertyValue;

            articles.Add(new Article
            {
                Title = titleValue?.Title?.FirstOrDefault()?.PlainText ?? "",
                Author = authorValue?.RichText?.FirstOrDefault()?.PlainText ?? "",
                Url = urlValue?.Url ?? "",
                Category = GetCategoryFromNotionCategory(categoryValue?.Select.Name.ToLower() ?? ""),
            });
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