using Microsoft.Extensions.Configuration;
using Model.Account;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Service.Token;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using Type.Account;

namespace Service.Account;

public interface IUser
{
    Task<List<Model.Account.UserPublic>> GetAsync(SearchParameters searchParameters, string userId);
    Task<Model.Account.UserPublic> GetAsync(string id, string userId);
    Task<string> GetTokenAsync(string foreignToken, string userId);

    Task CreateAsync(Model.Account.User user);
}

public class User : IUser
{
    public IConfiguration Configuration { get; set; }
    public IMongoCollection<Model.Account.User> UserCollection { get; set; }
    public int AuthenticationTokenExpirationInMinutes { get; set; }
    public string AuthenticationTokenSecret { get; set; }

    public User(IConfiguration configuration, IMongoClient mongoClient)
    {
        Configuration = configuration;

        AuthenticationTokenExpirationInMinutes = int.Parse(Configuration["Authentication:TokenExpirationInMinutes"]);
        AuthenticationTokenSecret = Configuration["Authentication:TokenSecret"];

        UserCollection = mongoClient.GetDatabase(nameof(Model.Account).ToLower()).GetCollection<Model.Account.User>(typeof(Model.Account.User).Name.ToLower());
    }

    public async Task<List<Model.Account.UserPublic>> GetAsync(SearchParameters searchParameters, string userId)
    {
        var administrator = Role.Administrator.ToString().ToLower();
        var creator = Role.Administrator.ToString().ToLower();

        var result = await UserCollection
            .AsQueryable()
            .Where(user =>
                searchParameters.IsCreator == null || user.Roles.Any(x => x == administrator)
                || searchParameters.IsCreator == null || user.Roles.Any(x => x == creator))
            .ToListAsync();

        var userPublic = new List<Model.Account.UserPublic>();

        foreach (var user in result)
        {
            userPublic.Add(new UserPublic(user));
        }

        return userPublic;
    }

    public async Task<Model.Account.UserPublic> GetAsync(string id, string userId)
    {
        var result = await UserCollection
            .AsQueryable()
            .Where(user => id == user.Id)
            .FirstOrDefaultAsync();

        return result == null ? null : new UserPublic(result);
    }

    public async Task<string> GetTokenAsync(string foreignToken, string userId)
    {
        var user = JwtToken.GetUser(foreignToken);

        var existingUser = await UserCollection.AsQueryable().Where(x => x.Email.ToLower() == user.Email.ToLower()).FirstOrDefaultAsync();

        if (existingUser == null)
            await CreateAsync(user);

        existingUser = await UserCollection.AsQueryable().Where(x => x.Email.ToLower() == user.Email.ToLower()).FirstOrDefaultAsync();

        return new JwtSecurityTokenHandler().WriteToken(JwtToken.CreateJwtToken(AuthenticationTokenSecret, existingUser));
    }

    public async Task CreateAsync(Model.Account.User user)
    {
        user.Email = user.Email.ToLower();
        user.Roles = new string[] { Role.User.ToString().ToLower(), Role.Creator.ToString().ToLower() };
        user.Created = DateTime.UtcNow;
        user.Modified = DateTime.UtcNow;

        await UserCollection.InsertOneAsync(user);
    }
}