using ExpenseTracker.Application.Accounts.Contracts.Requests;
using FluentValidation;

namespace ExpenseTracker.Application.Accounts.Validators;
public sealed class AddUserDtoValidator : AbstractValidator<AddUserRequestDto>
{
    public AddUserDtoValidator()
    {
        RuleFor(u => u.Firstname)
           .NotEmpty()
           .WithMessage("User must have a firstname.");

        RuleFor(u => u.Lastname)
            .NotEmpty()
            .WithMessage("User must have a lastname.");

        RuleFor(u => u.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email must be a valid address.");

        RuleFor(u => u.Password)
            .NotEmpty()
            .Matches("[A-Z]").WithMessage("Password must contain at least 1 uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least 1 number.")
            .Matches("[^A-Za-z0-9]").WithMessage("Password must contain at least 1 special character.")
            .NotEqual(u => u.Email).WithMessage("Password must not be the same as the email.")
            .NotEqual(u => u.Firstname).WithMessage("Password must not be the same as the firstname.")
            .NotEqual(u => u.Lastname).WithMessage("Password must not be the same as the lastname.")
            .MinimumLength(8)
            .WithMessage("Password already was previously used.");
    }
}
