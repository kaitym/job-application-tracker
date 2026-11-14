using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

// Checks the owner's login and hands out signed tokens
public class TokenService(AuthSettings settings)
{
    public const string Issuer = "job-tracker-api";

    // A token if the username and password match, otherwise null
    public LoginResponse? Login(LoginRequest request)
    {
        var usernameMatches = string.Equals(request.Username, settings.Username, StringComparison.Ordinal);
        var passwordMatches = Passwords.Verify(settings.PasswordHash, request.Password ?? "");

        if (!usernameMatches || !passwordMatches)
        {
            return null;
        }

        var expiresAt = DateTime.UtcNow.AddDays(settings.TokenDays);

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Issuer,
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Name, settings.Username)]),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(SigningKey(settings), SecurityAlgorithms.HmacSha256)
        });

        return new LoginResponse(token, expiresAt);
    }

    // The secret used to sign and check tokens
    public static SymmetricSecurityKey SigningKey(AuthSettings settings)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtKey));
    }
}
