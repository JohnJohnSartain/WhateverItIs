using Service.Content.Blog;
using Type.Account;

namespace WebApi.Endpoint.Content.Blog;

public static class Article
{
    private static readonly string endpointRoot = nameof(Content).ToLower() + "/" + nameof(Blog).ToLower() + "/" + typeof(Article).Name.ToLower();

    public static WebApplication Map(this WebApplication application)
    {
        application
            .MapGet(endpointRoot, (HttpContext httpContext, IArticle article, [AsParameters] Model.Content.Blog.SearchParameters searchParameters)
            => article.GetAsync(searchParameters, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])))
            .RequireAuthorization(Role.Administrator.ToString());

        application
            .MapGet(endpointRoot + "/public", (HttpContext httpContext, IArticle article, [AsParameters] Model.Content.Blog.SearchParameters searchParameters)
            => article.GetPublicAsync(searchParameters, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])));

        application
            .MapGet(endpointRoot + "/artificial-intelligence/{topic}", (HttpContext httpContext, IArticle article, string topic)
            => article.GetBaseFromArtificialIntelligenceAsync(topic, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])))
            .RequireAuthorization(Role.Creator.ToString());

        application
            .MapGet(endpointRoot + "/tag", (HttpContext httpContext, IArticle article, [AsParameters] Model.Content.Blog.SearchParameters searchParameters)
            => article.GetTagAsync(searchParameters, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])));

        application
            .MapGet(endpointRoot + "/{id}", (HttpContext httpContext, IArticle article, string id)
            => article.GetAsync(id, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])))
            .RequireAuthorization(Role.Creator.ToString());

        application
            .MapGet(endpointRoot + "/public/{id}", (HttpContext httpContext, IArticle article, string id)
            => article.GetPublicAsync(id, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])));

        application
            .MapPut(endpointRoot + "/{id}", (HttpContext httpContext, IArticle article, Model.Content.Blog.Article articleModel)
            => article.UpdateAsync(articleModel, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])))
            .RequireAuthorization(Role.Creator.ToString());

        application
            .MapPost(endpointRoot, (HttpContext httpContext, IArticle article, Model.Content.Blog.Article articleModel)
            => article.CreateAsync(articleModel, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])))
            .RequireAuthorization(Role.Creator.ToString());

        application
            .MapDelete(endpointRoot + "/{id}", (HttpContext httpContext, IArticle article, string id)
            => article.ArchiveAsync(id, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])))
            .RequireAuthorization(Role.Creator.ToString());

        return application;
    }
}