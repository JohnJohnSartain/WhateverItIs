using Model.Account;
using Service.Account;
using Service.Content.Blog;

namespace Service.Infrastructure;

public interface ISitemap
{
    Task<string> GetAsync();
}

public class Sitemap : ISitemap
{
    private IUser User { get; set; }
    private IArticle Article { get; set; }

    public Sitemap(IUser user, IArticle article)
    {
        User = user;
        Article = article;
    }

    public async Task<string> GetAsync()
    {
        var domainName = "https://whatever-it-is.com";
        var pages = new string[] { "/", "/home", "/trending", "/article", "/creators", "/our-story", "/write", "/account", "/privacy-policy", "/google-authentication", "/archive" };
        string sitemap = "";
        sitemap += "<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd\">";

        foreach (var page in pages)
        {
            sitemap += $"<url><loc>{domainName + page}</loc><lastmod>{DateTimeOffset.UtcNow.ToString("o")}</lastmod></url>";
        }

        var users = await User.GetAsync(new SearchParameters(), null);
        foreach (var user in users)
        {
            sitemap += $"<url><loc>{domainName + "/creator/" + user.Id}</loc><lastmod>{DateTimeOffset.UtcNow.ToString("o")}</lastmod></url>";
        }

        var articles = await Article.GetAsync(new Model.Content.Blog.SearchParameters(), null);
        foreach (var article in articles)
            sitemap += $"<url><loc>{domainName + "/article/" + article.Id}</loc><lastmod>{DateTimeOffset.UtcNow.ToString("o")}</lastmod></url>";

        sitemap += "</urlset>";

        return sitemap;
    }
}