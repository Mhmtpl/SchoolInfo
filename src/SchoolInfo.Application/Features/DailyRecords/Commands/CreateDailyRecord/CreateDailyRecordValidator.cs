using System;
using FluentValidation;

namespace SchoolInfo.Application.Features.DailyRecords.Commands.CreateDailyRecord;

/// <summary>
/// GÃ¼nlÃ¼k kayÄ±t oluÅŸturma isteÄŸinin kurallarÄ±.
/// </summary>
public class CreateDailyRecordValidator : AbstractValidator<CreateDailyRecordCommand>
{
    public CreateDailyRecordValidator()
    {
        RuleFor(v => v.StudentId).NotEmpty().WithMessage("Ã–ÄŸrenci Id boÅŸ olamaz.");
        RuleFor(v => v.Date).NotEmpty().WithMessage("Tarih boÅŸ olamaz.");
    }
}
