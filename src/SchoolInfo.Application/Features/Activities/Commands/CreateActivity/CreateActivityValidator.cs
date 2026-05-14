using FluentValidation;

namespace SchoolInfo.Application.Features.Activities.Commands.CreateActivity;

/// <summary>
/// Aktivite oluşturma kuralları.
/// </summary>
public class CreateActivityValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityValidator()
    {
        RuleFor(v => v.Title).NotEmpty().MaximumLength(100).WithMessage("Başlık boş olamaz ve en fazla 100 karakter olmalıdır.");
        RuleFor(v => v.ClassroomId).NotEmpty().WithMessage("Sınıf Id boş olamaz.");
    }
}
