using Service.Content.Blog;

namespace WebApi.Endpoint.Content.Blog;

public static class ArticleReaction
{
    private static readonly string endpointRoot = nameof(Content).ToLower() + "/" + nameof(Blog).ToLower() + "/" + typeof(Article).Name.ToLower() + "/Reaction";

    public static WebApplication Map(this WebApplication application)
    {
        application
           .MapPut(endpointRoot, (HttpContext httpContext, IArticleReaction articleView, Model.Content.Blog.ArticleReaction articleReaction)
           => articleView.UpsertAsync(articleReaction, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])));

        return application;
    }
}