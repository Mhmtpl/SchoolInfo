using FluentValidation;

namespace SchoolInfo.Application.Features.DailyRecords.Commands.UpdateDailyRecord;

/// <summary>
/// GÃ¼nlÃ¼k kaydÄ± gÃ¼ncelleme kurallarÄ±.
/// </summary>
public class UpdateDailyRecordValidator : AbstractValidator<UpdateDailyRecordCommand>
{
    public UpdateDailyRecordValidator()
    {
        RuleFor(v => v.DailyRecordId).NotEmpty().WithMessage("GÃ¼nlÃ¼k kayÄ±t Id boÅŸ olamaz.");
        RuleFor(v => v.WaterAmountInMilliliters).GreaterThanOrEqualTo(0).WithMessage("Su miktarÄ± negatif olamaz.");
    }
}
