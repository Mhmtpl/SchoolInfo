using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Students.Commands.CreateStudent;

public record CreateStudentCommand(string FirstName, string LastName, DateTime DateOfBirth, Guid ClassroomId) : IRequest<Guid>;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Guid>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateStudentCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Admin" && _currentUserService.Role != "Teacher")
            throw new UnauthorizedAccessException("Yetkiniz yok.");

        var classroom = await ((DbContext)_dbContext).Set<Classroom>().FirstOrDefaultAsync(c => c.Id == request.ClassroomId, cancellationToken);
        if (classroom == null || classroom.SchoolId != _currentUserService.SchoolId)
            throw new UnauthorizedAccessException("Gecersiz sinif.");

        var student = new Student(request.FirstName, request.LastName, request.DateOfBirth, request.ClassroomId);
        student.SchoolId = _currentUserService.SchoolId;

        await ((DbContext)_dbContext).Set<Student>().AddAsync(student, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return student.Id;
    }
}
