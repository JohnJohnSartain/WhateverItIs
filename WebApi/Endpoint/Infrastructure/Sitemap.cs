using Model.Account;
using Service.Infrastructure;

namespace WebApi.Endpoint.Infrastructure;

public static class Sitemap
{
    private static readonly string endpointRoot = nameof(Infrastructure).ToLower() + "/" + typeof(Sitemap).Name.ToLower();

    public static WebApplication Map(this WebApplication application)
    {
        application.MapGet(endpointRoot, (HttpContext httpContext, ISitemap sitemap, [AsParameters] SearchParameters searchParameters) => sitemap.GetAsync());

        return application;
    }
}