using ExpenseTracker.Application.Authentication.Contracts.Request;
using FluentValidation;

namespace ExpenseTracker.Application.Authorization.Validators;
internal sealed class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(p => p.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address must be valid.");

        RuleFor(p => p.Password)
            .NotEmpty()
            .WithMessage("Password must be provided.");
    }
}
