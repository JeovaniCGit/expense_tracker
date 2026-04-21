using ExpenseTracker.Application.Authentication.Contracts.Request;
using FluentValidation;

namespace ExpenseTracker.Application.Authentication.Validators;

public sealed class LoginDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginDtoValidator()
    {
        RuleFor(l => l.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email must be a valid address.");

        RuleFor(l => l.Password)
            .NotEmpty()
            .Matches("[A-Z]").WithMessage("Password must contain at least 1 uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least 1 number.")
            .Matches("[^A-Za-z0-9]").WithMessage("Password must contain at least 1 special character.")
            .NotEqual(l => l.Email).WithMessage("Password must not be the same as the email.")
            .MinimumLength(8)
            .WithMessage("Password already was previously used.");
    }
}
