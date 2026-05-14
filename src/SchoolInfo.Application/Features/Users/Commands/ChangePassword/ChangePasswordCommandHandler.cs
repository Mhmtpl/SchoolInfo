using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Exceptions;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ChangePasswordCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId && u.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (user == null)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("UserId", "KullanÄ±cÄ± bulunamadÄ±.") });

        bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash);
        if (!isOldPasswordValid)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("OldPassword", "Eski ÅŸifre hatalÄ±.") });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
