using Model.Account;
using Service.Account;

namespace WebApi.Endpoint.Account;

public static class User
{
    private static readonly string endpointRoot = nameof(Account).ToLower() + "/" + typeof(User).Name.ToLower();

    public static WebApplication Map(this WebApplication application)
    {
        application
            .MapGet(endpointRoot, (HttpContext httpContext, IUser user, [AsParameters] SearchParameters searchParameters)
            => user.GetAsync(searchParameters, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])));

        application
           .MapGet(endpointRoot + "/{id}", (HttpContext httpContext, IUser user, string id)
           => user.GetAsync(id, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])));

        application
            .MapGet(endpointRoot + "/me", (HttpContext httpContext, IUser user)
            => user.GetAsync(Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"]), Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])))
            .RequireAuthorization();

        application
            .MapPost(endpointRoot + "/token", (HttpContext httpContext, IUser user, SearchParameters searchParameters)
            => user.GetTokenAsync(searchParameters.Token, Service.Token.JwtToken.GetUserIdFromToken(httpContext.Request.Headers["Authorization"])));

        return application;
    }
}