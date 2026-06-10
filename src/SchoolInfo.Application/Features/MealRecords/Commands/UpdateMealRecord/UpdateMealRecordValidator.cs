using FluentValidation;

namespace SchoolInfo.Application.Features.MealRecords.Commands.UpdateMealRecord;

/// <summary>
/// Ã–ÄŸÃ¼n gÃ¼ncelleme kurallarÄ±.
/// </summary>
public class UpdateMealRecordValidator : AbstractValidator<UpdateMealRecordCommand>
{
    public UpdateMealRecordValidator()
    {
        RuleFor(v => v.StatusType).IsInEnum().WithMessage("Geçersiz öğün durumu.");
        
        RuleFor(v => v)
            .Must(v => v.MealRecordId != Guid.Empty || (v.StudentId.HasValue && !string.IsNullOrEmpty(v.MealName)))
            .WithMessage("Ya Öğün Kayıt Id'si ya da Öğrenci Id ve Öğün Adı belirtilmelidir.");
    }
}
