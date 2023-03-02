using MongoDB.Driver;
using Service.Account;
using Service.ArtificialIntelligence;
using Service.Content.Blog;
using WebApi.Startup;

var builder = WebApplication.CreateBuilder(args);

int AuthenticationTokenExpirationInMinutes = int.Parse(builder.Configuration["Authentication:TokenExpirationInMinutes"] ?? throw new ArgumentNullException(nameof(AuthenticationTokenExpirationInMinutes)));
string AuthenticationTokenSecret = builder.Configuration["Authentication:TokenSecret"] ?? throw new ArgumentNullException(nameof(AuthenticationTokenSecret));

string openAiApiKey = builder.Configuration["Key:OpenAi"] ?? throw new ArgumentNullException(nameof(openAiApiKey));
string openAiApiConnectionString = builder.Configuration["ConnectionString:OpenAi"] ?? throw new ArgumentNullException(nameof(openAiApiConnectionString));

string ConnectionStringWhateverItIsDatabase = builder.Configuration["ConnectionString:WhateverItIsDatabase"] ?? throw new ArgumentNullException(nameof(ConnectionStringWhateverItIsDatabase));

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(ConnectionStringWhateverItIsDatabase));
builder.Services.AddSingleton<IOpenAi, OpenAi>();
builder.Services.AddSingleton<IUser, User>();
builder.Services.AddSingleton<ICreator, Creator>();
builder.Services.AddSingleton<IArticle, Article>();
builder.Services.AddSingleton<IArticleReaction, ArticleReaction>();
builder.Services.AddSingleton<IArticleView, ArticleView>();
builder.Services.AddSingleton<Service.Infrastructure.ISitemap, Service.Infrastructure.Sitemap>();

string CorsPolicyName = "CorsOpenPolicy";

Cors.Configure(builder, CorsPolicyName);

builder.Services.AddEndpointsApiExplorer();

Swagger.Configure(builder);

Authentication.Configure(builder, AuthenticationTokenSecret, AuthenticationTokenExpirationInMinutes);

builder.Services.AddHttpClient();

var app = builder.Build();

Authentication.Configure(app);

Swagger.Configure(app);

Cors.Configure(app, CorsPolicyName);

app.UseHttpsRedirection();

WebApi.Endpoint.Account.User.Map(app);
WebApi.Endpoint.Content.Blog.Creator.Map(app);
WebApi.Endpoint.Content.Blog.Article.Map(app);
WebApi.Endpoint.Content.Blog.ArticleView.Map(app);
WebApi.Endpoint.Content.Blog.ArticleReaction.Map(app);
WebApi.Endpoint.Infrastructure.Sitemap.Map(app);

app.Run();