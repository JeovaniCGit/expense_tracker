using ExpenseTracker.Application.Records.Contracts.Requests;
using FluentValidation;

namespace ExpenseTracker.Application.Records.Validators;

//Keep in mind that here, should´ve created a DTO for the update, instead of passsing a list as a type
//TLDR: Create a DTO for an item and other for a colection
internal sealed class UpdateTransactionRecordsDtoValidator : AbstractValidator<List<UpdateTransactionRecordRequestDto>>
{
    public UpdateTransactionRecordsDtoValidator()
    {
        RuleForEach(r => r)
            .SetValidator(new UpdateTransactionRecordDtoValidator());
    }
}
