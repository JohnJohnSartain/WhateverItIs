using Model.Account;
using Service.Content.Blog;

namespace WebApi.Endpoint.Content.Blog;

public static class Creator
{
    private static readonly string endpointRoot = nameof(Content).ToLower() + "/" + nameof(Blog).ToLower() + "/" + typeof(Creator).Name.ToLower();

    public static WebApplication Map(this WebApplication application)
    {
        application
            .MapGet(endpointRoot, (HttpContext httpContext, ICreator creator, [AsParameters] SearchParameters searchParameters)
            => creator.GetAsync(searchParameters, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])));

        application
           .MapGet(endpointRoot + "/{id}", (HttpContext httpContext, ICreator creator, string id)
           => creator.GetAsync(id, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])));

        return application;
    }
}