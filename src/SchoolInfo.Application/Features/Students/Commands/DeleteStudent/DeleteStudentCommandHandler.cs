using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Exceptions;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Application.Features.Students.Commands.DeleteStudent;

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteStudentCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (student == null)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("StudentId", "Ã–ÄŸrenci bulunamadÄ±.") });

        student.Delete();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
