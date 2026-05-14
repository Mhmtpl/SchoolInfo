using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Exceptions;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Application.Features.Classrooms.Commands.DeleteClassroom;

public class DeleteClassroomCommandHandler : IRequestHandler<DeleteClassroomCommand>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteClassroomCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteClassroomCommand request, CancellationToken cancellationToken)
    {
        var classroom = await _dbContext.Classrooms
            .Include(c => c.Students)
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (classroom == null)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("ClassroomId", "SÄ±nÄ±f bulunamadÄ±.") });

        if (classroom.Students.Any(s => !s.IsDeleted))
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("ClassroomId", "SÄ±nÄ±fÄ±n iÃ§inde aktif Ã¶ÄŸrenciler bulunduÄŸu iÃ§in silinemez.") });

        classroom.Delete();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
