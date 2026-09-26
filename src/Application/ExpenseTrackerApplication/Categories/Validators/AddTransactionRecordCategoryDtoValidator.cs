using ExpenseTracker.Application.Categories.Contracts.Requests;
using FluentValidation;

namespace ExpenseTracker.Application.Categories.Validators;
public sealed class AddTransactionRecordCategoryDtoValidator : AbstractValidator<AddTransactionRecordCategoryRequestDto>
{
    public AddTransactionRecordCategoryDtoValidator()
    {
        RuleFor(tc => tc.CategoryName)
            .NotEmpty()
            .WithMessage("A category name must be provided.");
    }
}
