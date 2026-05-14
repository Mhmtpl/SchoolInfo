using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Students.Commands.UpdateStudent;

public record UpdateStudentCommand(Guid Id, string FirstName, string LastName, Guid ClassroomId) : IRequest;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateStudentCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Admin" && _currentUserService.Role != "Teacher")
            throw new UnauthorizedAccessException("Yetkiniz yok.");

        var student = await ((DbContext)_dbContext).Set<Student>()
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (student == null)
            throw new System.Collections.Generic.KeyNotFoundException("Ogrenci bulunamadi.");

        student.UpdateInfo(request.FirstName, request.LastName, student.DateOfBirth);
        student.ChangeClassroom(request.ClassroomId);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
