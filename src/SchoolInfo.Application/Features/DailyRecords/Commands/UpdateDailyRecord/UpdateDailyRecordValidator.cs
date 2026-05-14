using FluentValidation;

namespace SchoolInfo.Application.Features.DailyRecords.Commands.UpdateDailyRecord;

/// <summary>
/// Günlük kaydı güncelleme kuralları.
/// </summary>
public class UpdateDailyRecordValidator : AbstractValidator<UpdateDailyRecordCommand>
{
    public UpdateDailyRecordValidator()
    {
        RuleFor(v => v.DailyRecordId).NotEmpty().WithMessage("Günlük kayıt Id boş olamaz.");
        RuleFor(v => v.WaterAmountInMilliliters).GreaterThanOrEqualTo(0).WithMessage("Su miktarı negatif olamaz.");
    }
}
