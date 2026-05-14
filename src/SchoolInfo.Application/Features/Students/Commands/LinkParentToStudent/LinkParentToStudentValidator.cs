using FluentValidation;

namespace SchoolInfo.Application.Features.Students.Commands.LinkParentToStudent;

public class LinkParentToStudentValidator : AbstractValidator<LinkParentToStudentCommand>
{
    public LinkParentToStudentValidator()
    {
        RuleFor(v => v.StudentId).NotEmpty().WithMessage("Ã–ÄŸrenci ID boÅŸ olamaz.");
        RuleFor(v => v.ParentId).NotEmpty().WithMessage("Veli ID boÅŸ olamaz.");
    }
}
