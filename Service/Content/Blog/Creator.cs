using Model.Account;
using Service.Account;

namespace Service.Content.Blog;

public interface ICreator
{
    Task<List<UserPublic>> GetAsync(SearchParameters searchParameters, string userId);
    Task<UserPublic> GetAsync(string id, string userId);
}

public class Creator : ICreator
{
    public IUser User { get; set; }
    public IArticle Article { get; set; }

    public Creator(IUser user, IArticle article)
    {
        User = user;
        Article = article;
    }

    public async Task<List<UserPublic>> GetAsync(SearchParameters searchParameters, string userId)
    {
        var users = await User.GetAsync(new Model.Account.SearchParameters(), userId);

        var articles = await Article.GetAsync(new Model.Content.Blog.SearchParameters(), userId);
        var articleCreatedByIds = articles.Select(x => x.CreatedBy).Distinct().ToList();

        var creators = users.Where(x => articleCreatedByIds.Contains(x.Id)).ToList();

        return creators;
    }

    public async Task<UserPublic> GetAsync(string id, string userId)
    {
        if ((await Article.GetAsync(new Model.Content.Blog.SearchParameters { CreatorId = id }, userId)).Count == 0)
            throw new ArgumentException(id);

        return await User.GetAsync(id, userId);
    }
}