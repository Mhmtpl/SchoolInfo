using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.Classrooms.Commands.AssignTeacher;

/// <summary>
/// AssignTeacherToClassroomCommand handler'ı.
/// Sınıfa öğretmen atar; aynı öğretmen zaten atanmışsa tekrar eklemez.
/// </summary>
public class AssignTeacherToClassroomCommandHandler : IRequestHandler<AssignTeacherToClassroomCommand>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public AssignTeacherToClassroomCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(AssignTeacherToClassroomCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != UserRole.Admin.ToString())
            throw new UnauthorizedAccessException("Sadece admin sınıfa öğretmen atayabilir.");

        var classroom = await _dbContext.Classrooms
            .Include(c => c.Teachers)
            .FirstOrDefaultAsync(c => c.Id == request.ClassroomId && c.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (classroom == null)
            throw new KeyNotFoundException("Sınıf bulunamadı.");

        var teacher = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.TeacherId
                                   && u.SchoolId == _currentUserService.SchoolId
                                   && u.Role == UserRole.Teacher, cancellationToken);

        if (teacher == null)
            throw new KeyNotFoundException("Geçerli bir öğretmen bulunamadı.");

        classroom.AddTeacher(teacher);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
