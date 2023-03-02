namespace WebApi.Startup;

public static class Cors
{
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder, string corsPolicyName)
    {
        builder.Services.AddCors(o => o.AddPolicy(corsPolicyName, builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); }));

        return builder;
    }

    public static WebApplication Configure(this WebApplication application, string corsPolicyName)
    {
        application.UseCors(corsPolicyName);

        return application;
    }
}