using ExpenseTracker.Application.Authentication.Contracts.Request;
using FluentValidation;

namespace ExpenseTracker.Application.Authentication.Validators;

public sealed class RefreshDtoValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshDtoValidator()
    {
        RuleFor(r => r.RefreshToken)
            .NotEmpty()
            .WithMessage("Token value must be provided.");
    }
}
