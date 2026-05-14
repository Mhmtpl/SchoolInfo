using FluentValidation;

namespace SchoolInfo.Application.Features.Activities.Commands.CreateActivity;

/// <summary>
/// Aktivite oluÅŸturma kurallarÄ±.
/// </summary>
public class CreateActivityValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityValidator()
    {
        RuleFor(v => v.Title).NotEmpty().MaximumLength(100).WithMessage("BaÅŸlÄ±k boÅŸ olamaz ve en fazla 100 karakter olmalÄ±dÄ±r.");
        RuleFor(v => v.ClassroomId).NotEmpty().WithMessage("SÄ±nÄ±f Id boÅŸ olamaz.");
    }
}
