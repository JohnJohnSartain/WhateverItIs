using Service.Content.Blog;

namespace WebApi.Endpoint.Content.Blog;

public static class ArticleView
{
    private static readonly string endpointRoot = nameof(Content).ToLower() + "/" + nameof(Blog).ToLower() + "/" + typeof(Article).Name.ToLower() + "/View";

    public static WebApplication Map(this WebApplication application)
    {
        application
            .MapPost(endpointRoot + "/{articleId}", (HttpContext httpContext, IArticleView articleView, string articleId)
            => articleView.CreateAsync(articleId, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])));

        return application;
    }
}