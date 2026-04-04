using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Application.Records.Contracts.Requests;
using FluentValidation;

namespace ExpenseTracker.Application.Records.Validators;

//Keep in mind that here, should´ve created a DTO for the update, instead of passsing a list as a type
//TLDR: Create a DTO for an item and other for a collection
internal sealed class UpdateTransactionRecordsDtoValidator : AbstractValidator<List<UpdateTransactionRecordRequestDto>>
{
    public UpdateTransactionRecordsDtoValidator()
    {
        RuleForEach(r => r)
            .SetValidator(new UpdateTransactionRecordDtoValidator());

        RuleFor(x => x)
            .Must(HaveUniqueExternalIds)
            .WithMessage("Duplicate RecordExternalId values are not allowed.");
    }

    private bool HaveUniqueExternalIds(List<UpdateTransactionRecordRequestDto> list)
    {
        return list
            .Select(x => x.TransactionExternalId)
            .Distinct()
            .Count() == list.Count;
    }
}
