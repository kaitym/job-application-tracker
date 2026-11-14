using Microsoft.AspNetCore.Identity;

// Hash and check passwords (PBKDF2 with a random salt, from ASP.NET Core Identity)
public static class Passwords
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Owner = new();

    public static string Hash(string password)
    {
        return Hasher.HashPassword(Owner, password);
    }

    public static bool Verify(string hash, string password)
    {
        try
        {
            var result = Hasher.VerifyHashedPassword(Owner, hash, password);
            return result != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            // Not a valid hash in config
            return false;
        }
    }
}
