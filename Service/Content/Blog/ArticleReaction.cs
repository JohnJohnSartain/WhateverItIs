using Model.Content.Blog;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Data;

namespace Service.Content.Blog;

public interface IArticleReaction
{
    Task<List<Model.Content.Blog.ArticleReaction>> GetAsync(SearchParameters searchParameters, string userId);
    Task<Model.Content.Blog.ArticleReaction> GetAsync(string articleId, string userId);
    Task<List<ArticleReactionTotal>> GetTotalsAsync(string userId);
    Task<ArticleReactionTotal> GetTotalAsync(string articleId, string userId);

    Task<string> UpsertAsync(Model.Content.Blog.ArticleReaction articleReaction, string userId);
    Task<string> UpdateAsync(Model.Content.Blog.ArticleReaction articleReaction, string userId);

    Task<string> CreateAsync(Model.Content.Blog.ArticleReaction articleReaction, string userId);
}

public class ArticleReaction : IArticleReaction
{
    public IMongoCollection<Model.Content.Blog.ArticleReaction> ArticleReactionCollection { get; set; }

    public ArticleReaction(IMongoClient mongoClient)
    {
        ArticleReactionCollection = mongoClient.GetDatabase(nameof(Model.Content.Blog).ToLower()).GetCollection<Model.Content.Blog.ArticleReaction>(typeof(Model.Content.Blog.ArticleReaction).Name.ToLower());
    }

    public async Task<List<Model.Content.Blog.ArticleReaction>> GetAsync(SearchParameters searchParameters, string userId)
    {
        var result = await ArticleReactionCollection
            .AsQueryable()
            .Where(x => searchParameters.ArticleId != null ? searchParameters.ArticleId == x.ArticleId : true)
            .ToListAsync();

        return result.ToList();
    }

    public async Task<Model.Content.Blog.ArticleReaction> GetAsync(string articleId, string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(articleId);
        ArgumentException.ThrowIfNullOrEmpty(userId);

        var result = await ArticleReactionCollection
            .AsQueryable()
            .Where(article => articleId == article.ArticleId && userId == article.CreatedBy)
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<List<ArticleReactionTotal>> GetTotalsAsync(string userId)
    {
        var articleReactions = await GetAsync(new SearchParameters(), userId);

        var articleIds = articleReactions.Select(x => x.ArticleId).Distinct();

        var articleReactionsTotals = new List<ArticleReactionTotal>();

        foreach (var articleId in articleIds)
        {
            articleReactionsTotals.Add(new ArticleReactionTotal
            {
                Likes = articleReactions.Count(x => x.ArticleId == articleId && (x.IsLiked ?? false)),
                Dislikes = articleReactions.Count(x => x.ArticleId == articleId && (x.IsDisliked ?? false)),
                ArticleId = articleId
            });
        }

        return articleReactionsTotals;
    }

    public async Task<ArticleReactionTotal> GetTotalAsync(string articleId, string userId)
    {
        var articleReactions = await GetAsync(new SearchParameters { ArticleId = articleId }, userId);

        var likes = articleReactions.Count(x => x.ArticleId == articleId && (x.IsLiked ?? false));
        var dislikes = articleReactions.Count(x => x.ArticleId == articleId && (x.IsDisliked ?? false));

        return new ArticleReactionTotal { ArticleId = articleId, Likes = likes, Dislikes = dislikes };
    }

    public async Task<string> UpsertAsync(Model.Content.Blog.ArticleReaction articleReaction, string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(articleReaction.ArticleId);
        ArgumentException.ThrowIfNullOrEmpty(userId);

        var filter = Builders<Model.Content.Blog.ArticleReaction>.Filter.Eq(s => s.ArticleId, articleReaction.ArticleId) & Builders<Model.Content.Blog.ArticleReaction>.Filter.Eq(s => s.CreatedBy, userId);
        var existingArticleReaction = ArticleReactionCollection.Find(filter).FirstOrDefault();
        if (existingArticleReaction != null)
        {
            existingArticleReaction.IsLiked = articleReaction.IsLiked;
            existingArticleReaction.IsDisliked = articleReaction.IsDisliked;
            return await UpdateAsync(existingArticleReaction, userId);
        }
        else
            return await CreateAsync(articleReaction, userId);
    }

    public async Task<string> UpdateAsync(Model.Content.Blog.ArticleReaction articleReaction, string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(articleReaction.ArticleId);

        articleReaction.Modified = DateTime.UtcNow;
        articleReaction.ModifiedBy = userId;
        articleReaction.IsLiked = articleReaction.IsLiked ?? false;
        articleReaction.IsDisliked = articleReaction.IsDisliked ?? false;
        await ArticleReactionCollection.ReplaceOneAsync(Builders<Model.Content.Blog.ArticleReaction>.Filter.Eq(s => s.ArticleId, articleReaction.ArticleId) & Builders<Model.Content.Blog.ArticleReaction>.Filter.Eq(s => s.CreatedBy, userId), articleReaction);

        return articleReaction.Id;
    }

    public async Task<string> CreateAsync(Model.Content.Blog.ArticleReaction articleReaction, string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(articleReaction.ArticleId);
        articleReaction.Created = DateTime.UtcNow;
        articleReaction.Modified = DateTime.UtcNow;
        articleReaction.CreatedBy = userId;
        articleReaction.ModifiedBy = userId;
        articleReaction.IsLiked ??= false;
        articleReaction.IsDisliked ??= false;

        await ArticleReactionCollection.InsertOneAsync(articleReaction);
        return articleReaction.Id;
    }
}