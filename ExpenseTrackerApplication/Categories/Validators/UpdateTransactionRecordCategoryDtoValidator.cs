using ExpenseTracker.Application.Categories.Contracts.Requests;
using FluentValidation;

namespace ExpenseTracker.Application.Categories.Validators;
internal sealed class UpdateTransactionRecordCategoryDtoValidator : AbstractValidator<UpdateTransactionRecordCategoryRequestDto>
{
    public UpdateTransactionRecordCategoryDtoValidator()
    {
        RuleFor(tc => tc.CategoryName)
            .NotEmpty()
            .WithMessage("A category name must be provided.");

        RuleFor(tc => tc.CategoryExternalId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Invalid arguments");
    }
}
