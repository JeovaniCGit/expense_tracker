namespace ExpenseTracker.Application.Abstractions.GuidSeeder;

public sealed class GuidSeed
{
    public static Guid CreateGuidFromName(string input)
    {
        using var provider = System.Security.Cryptography.MD5.Create();
        var hash = provider.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
