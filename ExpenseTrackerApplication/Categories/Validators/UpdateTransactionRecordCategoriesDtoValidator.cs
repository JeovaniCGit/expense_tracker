using ExpenseTracker.Application.Categories.Contracts.Requests;
using FluentValidation;

namespace ExpenseTracker.Application.Categories.Validators;

//Keep in mind that here, should´ve created a DTO for the update, instead of passsing a list as a type
//TLDR: Create a DTO for an item and other for a colection
internal sealed class UpdateTransactionRecordCategoriesDtoValidator : AbstractValidator<List<UpdateTransactionRecordCategoryRequestDto>>
{
    public UpdateTransactionRecordCategoriesDtoValidator()
    {
        RuleForEach(c => c)
            .SetValidator(new UpdateTransactionRecordCategoryDtoValidator());
    }
}
