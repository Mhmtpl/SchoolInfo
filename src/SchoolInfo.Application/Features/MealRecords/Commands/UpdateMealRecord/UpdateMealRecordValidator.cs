using FluentValidation;

namespace SchoolInfo.Application.Features.MealRecords.Commands.UpdateMealRecord;

/// <summary>
/// Ã–ÄŸÃ¼n gÃ¼ncelleme kurallarÄ±.
/// </summary>
public class UpdateMealRecordValidator : AbstractValidator<UpdateMealRecordCommand>
{
    public UpdateMealRecordValidator()
    {
        RuleFor(v => v.MealRecordId).NotEmpty().WithMessage("Ã–ÄŸÃ¼n kayÄ±t Id boÅŸ olamaz.");
        RuleFor(v => v.StatusType).IsInEnum().WithMessage("GeÃ§ersiz Ã¶ÄŸÃ¼n durumu.");
    }
}
