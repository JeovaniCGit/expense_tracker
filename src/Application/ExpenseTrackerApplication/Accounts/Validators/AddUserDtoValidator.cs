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
    }
}
