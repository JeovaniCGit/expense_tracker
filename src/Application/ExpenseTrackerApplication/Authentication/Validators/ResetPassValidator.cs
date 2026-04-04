using ExpenseTracker.Application.Authentication.Contracts.Request;
using FluentValidation;

namespace ExpenseTracker.Application.Authentication.Validators;

internal sealed class ResetPassValidator : AbstractValidator<ResetPassRequestDto>
{
    public ResetPassValidator()
    {
        RuleFor(r => r.Password)
            .NotEmpty()
            .Matches("[A-Z]").WithMessage("Password must contain at least 1 uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least 1 number.")
            .Matches("[^A-Za-z0-9]").WithMessage("Password must contain at least 1 special character.")
            .MinimumLength(8)
            .WithMessage("Password already was previously used.");
    }
}
