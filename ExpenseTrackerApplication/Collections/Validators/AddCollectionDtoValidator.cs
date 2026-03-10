using ExpenseTracker.Application.Collections.Contracts.Requests;
using FluentValidation;

namespace ExpenseTracker.Application.Collections.Validators;
internal sealed class AddCollectionDtoValidator : AbstractValidator<AddCollectionRequestDto>
{
    public AddCollectionDtoValidator()
    {
        RuleFor(c => c.Description)
            .MinimumLength(200)
            .NotNull()
            .NotEmpty()
            .WithMessage("Collection must have a description.");

        RuleFor(c => c.EstimatedBudget)
            .GreaterThan(0)
            .WithMessage("Estimated budget must be bigger than 0.");

        RuleFor(c => c.RealBudget)
               .GreaterThan(0)
               .WithMessage("Real budget must be bigger than 0.");
    }
}
