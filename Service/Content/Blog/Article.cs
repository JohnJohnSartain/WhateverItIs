using Model.Content.Blog;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Service.Account;
using Service.ArtificialIntelligence;
using System.Data;

namespace Service.Content.Blog;

public interface IArticle
{
    Task<List<Model.Content.Blog.Article>> GetAsync(Model.Content.Blog.SearchParameters searchParameters, string userId);
    Task<List<Model.Content.Blog.ArticlePublic>> GetPublicAsync(Model.Content.Blog.SearchParameters searchParameters, string userId);
    Task<Model.Content.Blog.Article> GetBaseFromArtificialIntelligenceAsync(string topic, string userId);
    Task<List<string>> GetTagAsync(Model.Content.Blog.SearchParameters searchParameters, string userId);
    Task<Model.Content.Blog.Article> GetAsync(string id, string userId);
    Task<Model.Content.Blog.Article> GetPublicAsync(string id, string userId);

    Task<string> UpdateAsync(Model.Content.Blog.Article article, string userId);

    Task<string> CreateAsync(Model.Content.Blog.Article article, string userId);

    Task ArchiveAsync(string id, string userId);
    Task DeleteAsync(string id, string userId);
}

public class Article : IArticle
{
    public IMongoCollection<Model.Content.Blog.Article> ArticleCollection { get; set; }
    public IArticleReaction ArticleReaction { get; set; }
    public IArticleView ArticleView { get; set; }
    public IUser User { get; set; }
    public IOpenAi OpenAi { get; set; }

    public Article(IOpenAi openAi, IMongoClient mongoClient, IArticleReaction articleReaction, IArticleView articleView, IUser user)
    {
        OpenAi = openAi;
        ArticleCollection = mongoClient.GetDatabase(nameof(Model.Content.Blog).ToLower()).GetCollection<Model.Content.Blog.Article>(typeof(Model.Content.Blog.Article).Name.ToLower());
        ArticleReaction = articleReaction;
        ArticleView = articleView;
        User = user;
    }

    public async Task<Model.Content.Blog.Article> GetBaseFromArtificialIntelligenceAsync(string topic, string userId)
    {
        var article = new Model.Content.Blog.Article();

        var content = (await OpenAi.QueryAsync("Write an article about " + topic, 1500)).Choices[0].Text;
        article.Content = content.TrimStart('\r', '\n').TrimEnd('\r', '\n');

        var name = (await OpenAi.QueryAsync("Create a title for this article. " + article.Content, 100)).Choices[0].Text;
        article.Name = name.TrimStart('\r', '\n').TrimEnd('\r', '\n').Replace("\"", "");

        var description = (await OpenAi.QueryAsync("Briefly summarize this article. " + article.Content, 250)).Choices[0].Text;
        article.Description = description.TrimStart('\r', '\n').TrimEnd('\r', '\n');

        return article;
    }

    public async Task<List<Model.Content.Blog.Article>> GetAsync(Model.Content.Blog.SearchParameters searchParameters, string userId)
    {
        var builder = Builders<Model.Content.Blog.Article>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(searchParameters.CreatorId))
        {
            var creatorFilter = builder.Eq(x => x.CreatedBy, searchParameters.CreatorId);
            filter &= creatorFilter;
        }

        if (!string.IsNullOrWhiteSpace(searchParameters.Tag))
        {
            var tagFilter = builder.AnyEq(x => x.Tags, searchParameters.Tag);

            filter &= tagFilter;
        }

        var articles = await ArticleCollection.Find(filter).ToListAsync();

        articles.Take(searchParameters.Quantity ?? articles.Count);

        return articles.ToList();
    }

    public async Task<List<Model.Content.Blog.ArticlePublic>> GetPublicAsync(Model.Content.Blog.SearchParameters searchParameters, string userId)
    {
        var articles = await GetAsync(searchParameters, userId);

        var articlesPublic = new List<ArticlePublic>();

        foreach (var result in articles)
            articlesPublic.Add(new ArticlePublic(result));

        await FillArticlePublicWithArticleReactionsAsync(articlesPublic, userId);
        await FillArticlePublicWithUsersAsync(articlesPublic, userId);
        await FillArticlePublicWithViews(articlesPublic, userId);

        if (searchParameters.SortByMostViewed != null && searchParameters.SortByMostViewed.Value)
            articlesPublic = articlesPublic.OrderByDescending(x => x.Views).ToList();

        if (searchParameters.SortByMostPopular != null && searchParameters.SortByMostPopular.Value)
            articlesPublic = articlesPublic.OrderByDescending(x => x.Likes).ToList();

        if (searchParameters.Quantity != null)
            return articlesPublic.Take(searchParameters.Quantity.Value).ToList();

        return articlesPublic.ToList();
    }

    public async Task<List<string>> GetTagAsync(Model.Content.Blog.SearchParameters searchParameters, string userId)
    {
        var results = await ArticleCollection.AsQueryable().ToListAsync();

        var articleTags = new List<string>();

        foreach (var result in results)
        {
            var tags = result.Tags;

            if (tags != null)
                foreach (var tag in tags)
                    articleTags.Add(tag);
        }

        return articleTags;
    }

    private bool ShouldInclude(Model.Content.Blog.Article article, Model.Content.Blog.SearchParameters searchParameters)
    {
        var shouldInclude = false;

        if (searchParameters.UserId != null && searchParameters.UserId == article.CreatedBy) shouldInclude = true;
        if (searchParameters.Tag != null && article.Tags.Select(s => s.ToLowerInvariant()).Contains(searchParameters.Tag)) shouldInclude = true;

        return shouldInclude;
    }

    private async Task<List<ArticlePublic>> FillArticlePublicWithArticleReactionsAsync(List<ArticlePublic> articles, string userId)
    {
        var reactions = await ArticleReaction.GetTotalsAsync(userId);

        foreach (var article in articles)
        {
            var reaction = reactions.FirstOrDefault(x => x.ArticleId == article.Id);

            article.Likes = reaction != null ? reaction.Likes : 0;
            article.Dislikes = reaction != null ? reaction.Dislikes : 0;
        }

        return articles;
    }

    private async Task<List<ArticlePublic>> FillArticlePublicWithUsersAsync(List<ArticlePublic> articles, string userId)
    {
        var users = await User.GetAsync(new Model.Account.SearchParameters(), userId);

        foreach (var article in articles)
        {
            var user = users.FirstOrDefault(x => x.Id == article.CreatedBy);
            article.CreatorName = user != null ? user.Name : "No name given";
            article.CreatorProfileImageUrl = user != null ? user.ProfileImageUrl : "No photo given";
        }

        return articles;
    }

    private async Task<List<ArticlePublic>> FillArticlePublicWithViews(List<ArticlePublic> articles, string userId)
    {
        var views = await ArticleView.GetTotalsAsync(userId);

        foreach (var article in articles)
        {
            var view = views.FirstOrDefault(x => x.ArticleId == article.Id);
            if (view != null)
                article.Views = view.Views;
        }

        return articles;
    }

    public async Task<Model.Content.Blog.Article> GetAsync(string id, string userId)
    {
        try
        {
            var result = await ArticleCollection
            .AsQueryable()
            .Where(article => id == article.Id && userId == article.CreatedBy)
            .FirstAsync();

            return result;
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException("Editing article of another user is prohibited.", exception);
        }
    }

    private async Task<Model.Content.Blog.Article> GetAsync(string id)
    {
        var result = await ArticleCollection
            .AsQueryable()
            .Where(article => id == article.Id)
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<Model.Content.Blog.Article> GetPublicAsync(string id, string userId)
    {
        var article = await GetAsync(id);
        if (article == null)
            throw new KeyNotFoundException("No articles exist with the id of: " + id);

        var articlePublic = new ArticlePublic(article);

        var articleReactions = await ArticleReaction.GetTotalAsync(id, userId);
        var articleReaction = string.IsNullOrEmpty(userId) ? null : await ArticleReaction.GetAsync(id, userId);
        var articleCreator = await User.GetAsync(articlePublic.CreatedBy, articlePublic.CreatedBy);
        var articleView = await ArticleView.GetTotalAsync(articlePublic.Id, articlePublic.CreatedBy);

        articlePublic.Likes = articleReactions != null ? articleReactions.Likes : 0;
        articlePublic.Dislikes = articleReactions != null ? articleReactions.Dislikes : 0;
        articlePublic.IsLiked = articleReaction?.IsLiked;
        articlePublic.IsDisliked = articleReaction?.IsDisliked;
        articlePublic.CreatorName = articleCreator != null ? articleCreator.Name : "Account Deleted";
        articlePublic.CreatorProfileImageUrl = articleCreator != null ? articleCreator.ProfileImageUrl : "";
        articlePublic.Views = articleView.Views;

        return articlePublic;
    }

    public async Task<string> UpdateAsync(Model.Content.Blog.Article article, string userId)
    {
        article.Modified = DateTime.UtcNow;
        article.ModifiedBy = userId;

        var filter = Builders<Model.Content.Blog.Article>.Filter.Eq(s => s.Id, article.Id) & Builders<Model.Content.Blog.Article>.Filter.Eq(s => s.CreatedBy, userId);

        var result = await ArticleCollection.ReplaceOneAsync(filter, article);

        return article.Id;
    }

    public async Task<string> CreateAsync(Model.Content.Blog.Article article, string userId)
    {
        article.Created = DateTime.UtcNow;
        article.Modified = DateTime.UtcNow;
        article.CreatedBy = userId;
        article.ModifiedBy = userId;

        await ArticleCollection.InsertOneAsync(article);

        return article.Id;
    }

    public async Task ArchiveAsync(string id, string userId)
    {
        var filter = Builders<Model.Content.Blog.Article>.Filter.Eq(s => s.Id, id) & Builders<Model.Content.Blog.Article>.Filter.Eq(s => s.CreatedBy, userId);
        var update = Builders<Model.Content.Blog.Article>.Update.Set(s => s.IsDeleted, true).Set(s => s.ModifiedBy, userId).Set(s => s.Modified, DateTime.UtcNow);

        var result = await ArticleCollection.UpdateOneAsync(filter, update);
    }

    public async Task DeleteAsync(string id, string userId)
    {
        var filter = Builders<Model.Content.Blog.Article>.Filter.Eq(s => s.Id, id);

        await ArticleCollection.DeleteOneAsync(filter);
    }
}