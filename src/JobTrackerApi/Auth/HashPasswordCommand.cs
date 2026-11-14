// `dotnet run -- hash-password`: prints a hash for Auth:PasswordHash
public static class HashPasswordCommand
{
    public static void Run()
    {
        Console.Write("Password: ");
        var password = ReadHidden();

        if (password.Length < 8)
        {
            Console.Error.WriteLine("Use at least 8 characters.");
            Environment.ExitCode = 1;
            return;
        }

        Console.WriteLine(Passwords.Hash(password));
    }

    // Read a line without showing what's typed
    private static string ReadHidden()
    {
        if (Console.IsInputRedirected)
        {
            return Console.ReadLine() ?? "";
        }

        var password = "";
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return password;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0) password = password[..^1];
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password += key.KeyChar;
            }
        }
    }
}
