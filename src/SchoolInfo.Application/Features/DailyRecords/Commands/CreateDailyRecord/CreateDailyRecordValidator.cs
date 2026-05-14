using System;
using FluentValidation;

namespace SchoolInfo.Application.Features.DailyRecords.Commands.CreateDailyRecord;

/// <summary>
/// Günlük kayıt oluşturma isteğinin kuralları.
/// </summary>
public class CreateDailyRecordValidator : AbstractValidator<CreateDailyRecordCommand>
{
    public CreateDailyRecordValidator()
    {
        RuleFor(v => v.StudentId).NotEmpty().WithMessage("Öğrenci Id boş olamaz.");
        RuleFor(v => v.Date).NotEmpty().WithMessage("Tarih boş olamaz.");
    }
}
