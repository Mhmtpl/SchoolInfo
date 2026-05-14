using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Exceptions;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Application.Features.Users.Commands.UpdateCurrentUser;

public class UpdateCurrentUserCommandHandler : IRequestHandler<UpdateCurrentUserCommand>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCurrentUserCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId && u.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (user == null)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("UserId", "KullanÄ±cÄ± bulunamadÄ±.") });

        user.UpdateDetails(request.FirstName, request.LastName, request.Email);
        
        if (!string.IsNullOrEmpty(request.FcmToken))
        {
            user.UpdateFcmToken(request.FcmToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
