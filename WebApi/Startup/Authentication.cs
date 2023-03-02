using Microsoft.IdentityModel.Tokens;
using System.Text;
using Type.Account;

namespace WebApi.Startup;

public static class Authentication
{
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder, string authenticationTokenSecret, int authenticationTokenExpirationInMinutes)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "Jwt";
            options.DefaultChallengeScheme = "Jwt";
        }).AddJwtBearer(
            "Jwt",
            options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationTokenSecret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(authenticationTokenExpirationInMinutes)
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Role.Administrator.ToString(), authBuilder => { authBuilder.RequireRole(Role.Administrator.ToString().ToLower()); });
            options.AddPolicy(Role.Creator.ToString(), authBuilder => { authBuilder.RequireRole(Role.Creator.ToString().ToLower()); });
            options.AddPolicy(Role.User.ToString(), authBuilder => { authBuilder.RequireRole(Role.User.ToString().ToLower()); });
        });

        return builder;
    }

    public static WebApplication Configure(this WebApplication application)
    {
        application.UseAuthentication();
        application.UseAuthorization();

        return application;
    }
}