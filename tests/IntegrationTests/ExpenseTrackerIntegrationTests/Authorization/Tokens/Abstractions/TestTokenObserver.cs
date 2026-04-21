public class TestTokenObserver : IVerificationTokenObserver
{
    public Dictionary<string, string> Tokens { get; } = new();

    public void OnTokenGenerated(string email, string token)
    {
        Tokens.Add(email, token);
    }
}