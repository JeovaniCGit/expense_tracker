using ExpenseTracker.Application.Records.Contracts.Requests;
using FluentValidation;

namespace ExpenseTracker.Application.Records.Validators;
internal sealed class UpdateTransactionRecordDtoValidator : AbstractValidator<UpdateTransactionRecordRequestDto>
{
    public UpdateTransactionRecordDtoValidator()
    {
        RuleFor(tr => tr.TransactionValue)
          .NotEmpty()
          .GreaterThan(0)
          .WithMessage("Transaction value must be greater than 0.");

        RuleFor(tr => tr.TransactionExternalId)
             .NotEmpty()
             .Must(id => Guid.TryParse(id, out _))
             .WithMessage("Invalid arguments.");
    }
}
