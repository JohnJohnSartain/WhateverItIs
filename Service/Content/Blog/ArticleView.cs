using Model.Content.Blog;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Data;

namespace Service.Content.Blog;

public interface IArticleView
{
    Task<List<Model.Content.Blog.ArticleView>> GetAsync(SearchParameters searchParameters, string userId);
    Task<ArticleViewTotal> GetTotalAsync(string articleId, string userId);
    Task<List<ArticleViewTotal>> GetTotalsAsync(string userId);

    Task CreateAsync(string articleId, string userId);
}

public class ArticleView : IArticleView
{
    public IMongoCollection<Model.Content.Blog.ArticleView> ArticleViewCollection { get; set; }

    public ArticleView(IMongoClient mongoClient)
    {
        ArticleViewCollection = mongoClient.GetDatabase(nameof(Model.Content.Blog).ToLower()).GetCollection<Model.Content.Blog.ArticleView>(typeof(Model.Content.Blog.ArticleView).Name.ToLower());
    }

    public async Task<List<Model.Content.Blog.ArticleView>> GetAsync(SearchParameters searchParameters, string userId)
    {
        var result = await ArticleViewCollection.AsQueryable().ToListAsync();

        return result.ToList();
    }

    public async Task<ArticleViewTotal> GetTotalAsync(string articleId, string userId)
    {
        var articleViews = await ArticleViewCollection.AsQueryable().Where(x => articleId == x.ArticleId).ToListAsync();

        return new ArticleViewTotal
        {
            Views = articleViews.Count(x => x.ArticleId.Equals(articleId)),
            ArticleId = articleId
        };
    }

    public async Task<List<ArticleViewTotal>> GetTotalsAsync(string userId)
    {
        var articleViews = await GetAsync(new SearchParameters(), userId);

        var articleIds = articleViews.Select(x => x.ArticleId).Distinct();

        var articleViewTotals = new List<ArticleViewTotal>();

        foreach (var articleId in articleIds)
        {
            articleViewTotals.Add(new ArticleViewTotal
            {
                Views = articleViews.Count(x => x.ArticleId == articleId),
                ArticleId = articleId
            });
        }

        return articleViewTotals;
    }

    public async Task CreateAsync(string articleId, string userId)
    {
        var view = new Model.Content.Blog.ArticleView
        {
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            CreatedBy = userId ?? null,
            ModifiedBy = userId ?? null,
            ArticleId = articleId
        };

        await ArticleViewCollection.InsertOneAsync(view);
    }
}