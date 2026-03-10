using ExpenseTracker.Application.Collections.Contracts.Requests;
using FluentValidation;

namespace ExpenseTracker.Application.Collections.Validators;
internal sealed class UpdateCollectionDtoValidator : AbstractValidator<UpdateCollectionRequestDto>
{
    public UpdateCollectionDtoValidator()
    {
        RuleFor(c => c.CollectionExternalId)
            .NotEmpty()
            .WithMessage("Invalid arguments.");

        RuleFor(c => c.Description)
            .NotEmpty()
            .When(c => c.Description is not null)
            .MinimumLength(200)
            .WithMessage("Collection must have a description.");

        RuleFor(u => u.EstimatedBudget)
            .NotEmpty()
            .When(c => c.EstimatedBudget is not null)
            .GreaterThan(0)
            .WithMessage("Estimated budget must be bigger than 0.");

        RuleFor(u => u.RealBudget)
            .NotEmpty()
            .When(c => c.RealBudget is not null)
            .GreaterThan(0)
            .WithMessage("Real budget must be bigger than 0.");
    }
}
