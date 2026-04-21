public class NoopVerificationTokenObserver : IVerificationTokenObserver
{
    public void OnTokenGenerated(string email, string token) { }
}