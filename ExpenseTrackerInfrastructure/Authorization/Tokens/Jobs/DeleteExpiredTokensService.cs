using ExpenseTracker.Domain.Authorization.Tokens.Repository;
using ExpenseTracker.Application.Authorization.Tokens.Jobs;

namespace ExpenseTracker.Infrastructure.Authorization.Tokens.Jobs;

public sealed class DeleteExpiredTokensJob : IDeleteExpiredTokensService
{
    internal const string Name = nameof(DeleteExpiredTokensJob);
    private readonly ITokenRepository _tokenRepository;

    public DeleteExpiredTokensJob(ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public async Task DeleteExpiredTokens()
    {
        await _tokenRepository.DeleteExpiredTokens();
    }
}
