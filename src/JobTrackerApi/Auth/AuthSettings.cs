// Owner login settings, read from config ("Auth" section)
public record AuthSettings(string Username, string PasswordHash, string JwtKey, int TokenDays)
{
    // Read and check the settings, fail fast if anything is missing
    public static AuthSettings Load(IConfiguration config)
    {
        var section = config.GetSection("Auth");
        var username = section["Username"];
        var passwordHash = section["PasswordHash"];
        var jwtKey = section["JwtKey"];
        var tokenDays = section.GetValue("TokenDays", 7);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new InvalidOperationException(
                "Auth:Username and Auth:PasswordHash must be set. Run `dotnet run --project src/JobTrackerApi -- hash-password` to make a hash.");
        }

        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
        {
            throw new InvalidOperationException("Auth:JwtKey must be set and at least 32 characters long.");
        }

        return new AuthSettings(username, passwordHash, jwtKey, tokenDays);
    }
}
