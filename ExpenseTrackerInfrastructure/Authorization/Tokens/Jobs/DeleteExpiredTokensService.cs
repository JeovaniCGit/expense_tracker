using ExpenseTracker.Application.Authorization.Tokens.Jobs;
using ExpenseTracker.Domain.Authorization.Tokens.Repository;
using Hangfire;

namespace ExpenseTracker.Infrastructure.Authorization.Tokens.Jobs;

public sealed class DeleteExpiredTokensJob : IDeleteExpiredTokensService
{
    internal const string Name = nameof(DeleteExpiredTokensJob);
    private readonly ITokenRepository _tokenRepository;

    public DeleteExpiredTokensJob(ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    // Prevent job from being executed more than once at the same time
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task DeleteExpiredTokens()
    {
        await _tokenRepository.DeleteExpiredTokens();
    }
}
