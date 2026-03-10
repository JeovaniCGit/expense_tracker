namespace ExpenseTracker.Application.Authorization.Tokens.Jobs;

public interface IDeleteExpiredTokensService
{
    Task DeleteExpiredTokens();
}
