using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Exceptions;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Application.Features.Activities.Commands.DeleteActivity;

public class DeleteActivityCommandHandler : IRequestHandler<DeleteActivityCommand>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteActivityCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _dbContext.Activities
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (activity == null)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("ActivityId", "Etkinlik bulunamadÄ±.") });

        if (activity.CompletedAt.HasValue)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("ActivityId", "TamamlanmÄ±ÅŸ etkinlikler silinemez.") });

        if (_currentUserService.Role == Domain.Enums.UserRole.Teacher.ToString())
        {
            var classroom = await _dbContext.Classrooms
                .Include(c => c.Teachers)
                .FirstOrDefaultAsync(c => c.Id == activity.ClassroomId, cancellationToken);

            var isAssigned = classroom != null && classroom.Teachers.Any(t => t.Id == _currentUserService.UserId);
            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Bu etkinliği silme yetkiniz yok.");
            }
        }

        activity.Delete();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
