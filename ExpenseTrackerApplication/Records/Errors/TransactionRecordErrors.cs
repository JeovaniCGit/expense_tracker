using ErrorOr;

namespace ExpenseTracker.Application.Records.Errors;
public sealed class TransactionRecordErrors
{
    public static Error NotFound =>
        Error.NotFound("TransactionRecord.NotFound", "Record not found.");

    public static Error NotOwner =>
        Error.Unauthorized("TransactionRecord.NotOwner", "Unauthorized.");

    public static Error InvalidArgs =>
        Error.Validation("TransactionRecord.InvalidArgs", "Error, invalid arguments.");

    public static Error Unauthorized =>
        Error.Unauthorized("TransactionRecord.Unauthorized", "Error, unauthorized action.");
}
