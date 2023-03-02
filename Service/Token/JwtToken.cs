using Microsoft.IdentityModel.Tokens;
using Model.Account;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Service.Token;

public static class JwtToken
{
    public static string GetUserIdFromToken(string authorizationToken)
    {
        if (authorizationToken == null) return null;

        try
        {
            string token = GetTokenFromAuthenticationBearerString(authorizationToken);

            var claims = GetClaimsFromToken(token);

            return claims.FirstOrDefault(token => token.Type == "id").Value.ToLower();
        }
        catch
        {
            return null;
        }
    }

    public static JwtSecurityToken CreateJwtToken(string jwtSecret, Model.Account.User user, int minutesTillExpiration = 3000)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(user));

        return new(GetJwtHeader(jwtSecret), GetJwtPayload(minutesTillExpiration, user));
    }

    public static User GetUser(string token)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(token);

        return GetUserFromClaims(GetClaimsFromToken(token));
    }

    private static string GetTokenFromAuthenticationBearerString(string token)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(token);

        return token.Substring(7, token.Length - 7);
    }

    public static IEnumerable<Claim> GetClaimsFromToken(string token)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(token);

        return new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
    }

    private static JwtHeader GetJwtHeader(string jwtSecret)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(jwtSecret);

        return new(new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)), SecurityAlgorithms.HmacSha256));
    }

    private static JwtPayload GetJwtPayload(int minutesTillExpiration, Model.Account.User user = null) => new(CreateJwtClaims(minutesTillExpiration, user));


    private static IEnumerable<Claim> CreateJwtClaims(int minutesTillExpiration, Model.Account.User user) => CreateClaimsFromUserModel(user, minutesTillExpiration);

    private static User GetUserFromClaims(IEnumerable<Claim> claims)
    {
        return new Model.Account.User
        {
            Email = claims.FirstOrDefault(token => token.Type == "email").Value.ToLower(),
            FirstName = claims.FirstOrDefault(token => token.Type == "given_name").Value,
            ProfileImageUrl = claims.FirstOrDefault(token => token.Type == "picture").Value,
            Issuer = claims.FirstOrDefault(token => token.Type == "iss").Value,
            LastName = claims.FirstOrDefault(token => token.Type == "family_name").Value,
            Password = null
        };
    }

    private static Claim[] CreateClaimsFromUserModel(Model.Account.User user, int minutesTillExpiration = 3000)
    {
        var claims = new List<Claim>
        {
            new Claim(nameof(Model.Account.User.Id).ToLower(), user.Id ?? "No user id found"),
            new Claim(JwtRegisteredClaimNames.Name, user.FirstName + " " + user.LastName),
            new Claim(JwtRegisteredClaimNames.GivenName,  user.FirstName ?? "No first name given"),
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? "No last name found"),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "No email found"),
            new Claim(nameof(Model.Account.User.ProfileImageUrl).ToLower(), user.ProfileImageUrl),
            new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddMinutes(minutesTillExpiration)).ToUnixTimeSeconds().ToString())
        };

        foreach (var role in user.Roles ??= Array.Empty<string>())
            claims.Add(new Claim(nameof(role), role.ToLower()));

        return claims.ToArray();
    }
}