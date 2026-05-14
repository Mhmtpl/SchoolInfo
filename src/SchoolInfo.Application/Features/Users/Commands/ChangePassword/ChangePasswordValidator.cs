using FluentValidation;

namespace SchoolInfo.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(v => v.OldPassword).NotEmpty().WithMessage("Eski ÅŸifre boÅŸ olamaz.");
        RuleFor(v => v.NewPassword).NotEmpty().MinimumLength(6).WithMessage("Yeni ÅŸifre en az 6 karakter olmalÄ±dÄ±r.");
    }
}
