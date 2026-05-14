using FluentValidation;

namespace SchoolInfo.Application.Features.Schools.Commands.CreateSchool;

public class CreateSchoolValidator : AbstractValidator<CreateSchoolCommand>
{
    public CreateSchoolValidator()
    {
        RuleFor(v => v.Name).NotEmpty().WithMessage("Okul adi bos olamaz.");
    }
}
