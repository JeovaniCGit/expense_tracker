using BCrypt.Net;
using ExpenseTracker.Application.Authorization.BCryptLib;

namespace ExpenseTracker.Infrastructure.Authorization.BCryptLib;
public sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
