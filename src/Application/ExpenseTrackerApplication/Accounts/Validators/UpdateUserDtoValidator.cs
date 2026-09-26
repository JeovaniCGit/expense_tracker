using ExpenseTracker.Application.Accounts.Contracts.Requests;
using FluentValidation;

namespace ExpenseTracker.Application.Accounts.Validators;
public sealed class UpdateUserDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(u => u.Firstname)
            .NotEmpty()
            .When(u => u.Firstname is not null)
            .WithMessage("User must have a firstname.");

        RuleFor(u => u.Lastname)
            .NotEmpty()
            .When(u => u.Lastname is not null)
            .WithMessage("User must have a lastname.");

        RuleFor(u => u.Email)
            .EmailAddress()
            .When(u => u.Email is not null)
            .WithMessage("Email must be a valid address.");
    }
}
