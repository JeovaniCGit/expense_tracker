public interface IVerificationTokenObserver
{
    void OnTokenGenerated(string email, string token);
}