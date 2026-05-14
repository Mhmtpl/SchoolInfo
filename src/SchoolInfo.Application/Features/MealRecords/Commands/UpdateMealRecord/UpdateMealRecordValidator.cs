using FluentValidation;

namespace SchoolInfo.Application.Features.MealRecords.Commands.UpdateMealRecord;

/// <summary>
/// Öğün güncelleme kuralları.
/// </summary>
public class UpdateMealRecordValidator : AbstractValidator<UpdateMealRecordCommand>
{
    public UpdateMealRecordValidator()
    {
        RuleFor(v => v.MealRecordId).NotEmpty().WithMessage("Öğün kayıt Id boş olamaz.");
        RuleFor(v => v.StatusType).IsInEnum().WithMessage("Geçersiz öğün durumu.");
    }
}
