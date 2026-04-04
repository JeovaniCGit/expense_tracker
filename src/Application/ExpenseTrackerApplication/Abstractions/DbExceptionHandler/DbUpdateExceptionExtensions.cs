using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Abstractions.DbExceptionHandler;

public static class DbUpdateExceptionExtensions
{
    public static bool IsUniqueConstraintViolation(this DbUpdateException ex)
    {
        return ex.InnerException is Npgsql.PostgresException pgEx &&
          pgEx.SqlState == "23505";
    }
}
