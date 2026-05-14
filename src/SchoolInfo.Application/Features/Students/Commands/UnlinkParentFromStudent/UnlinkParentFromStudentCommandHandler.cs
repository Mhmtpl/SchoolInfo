using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Exceptions;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Application.Features.Students.Commands.UnlinkParentFromStudent;

public class UnlinkParentFromStudentCommandHandler : IRequestHandler<UnlinkParentFromStudentCommand>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UnlinkParentFromStudentCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UnlinkParentFromStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _dbContext.Students
            .Include(s => s.Parents)
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && s.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (student == null)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("StudentId", "Ã–ÄŸrenci bulunamadÄ±.") });

        var parent = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.ParentId && u.SchoolId == _currentUserService.SchoolId && u.Role == Domain.Enums.UserRole.Parent, cancellationToken);

        if (parent == null)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("ParentId", "GeÃ§erli bir veli bulunamadÄ±.") });

        student.RemoveParent(parent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
