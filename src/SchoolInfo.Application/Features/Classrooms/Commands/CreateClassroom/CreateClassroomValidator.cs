using FluentValidation;

namespace SchoolInfo.Application.Features.Classrooms.Commands.CreateClassroom;

public class CreateClassroomValidator : AbstractValidator<CreateClassroomCommand>
{
    public CreateClassroomValidator()
    {
        RuleFor(v => v.Name).NotEmpty().WithMessage("Sinif adi bos olamaz.");
    }
}
