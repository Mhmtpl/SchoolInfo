using FluentValidation;

namespace SchoolInfo.Application.Features.Users.Commands.UpdateCurrentUser;

public class UpdateCurrentUserValidator : AbstractValidator<UpdateCurrentUserCommand>
{
    public UpdateCurrentUserValidator()
    {
        RuleFor(v => v.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.LastName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Email).NotEmpty().EmailAddress();
    }
}
