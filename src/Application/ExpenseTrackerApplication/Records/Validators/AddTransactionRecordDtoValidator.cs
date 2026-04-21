using ExpenseTracker.Application.Records.Contracts.Requests;
using FluentValidation;

namespace ExpenseTracker.Application.Records.Validators;
public sealed class AddTransactionRecordDtoValidator : AbstractValidator<AddTransactionRecordRequestDto>
{
    public AddTransactionRecordDtoValidator()
    {
        RuleFor(tr => tr.TransactionValue)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Transaction value must be greater than 0.");

        RuleFor(tr => tr.TransactionUserExternalId)
             .NotEmpty()
             .Must(id => Guid.TryParse(id, out _))
             .WithMessage("Invalid arguments.");

        RuleFor(tr => tr.TransactionCategoryExternalId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Invalid arguments.");

        RuleFor(tr => tr.TransactionCollectionExternalId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Invalid arguments.");
    }
}
