using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.Classrooms.Commands.RemoveTeacher;

/// <summary>
/// RemoveTeacherFromClassroomCommand handler'ı.
/// Sınıftan öğretmeni kaldırır.
/// </summary>
public class RemoveTeacherFromClassroomCommandHandler : IRequestHandler<RemoveTeacherFromClassroomCommand>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public RemoveTeacherFromClassroomCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(RemoveTeacherFromClassroomCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != UserRole.Admin.ToString())
            throw new UnauthorizedAccessException("Sadece admin sınıftan öğretmen çıkarabilir.");

        var classroom = await _dbContext.Classrooms
            .Include(c => c.Teachers)
            .FirstOrDefaultAsync(c => c.Id == request.ClassroomId && c.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (classroom == null)
            throw new KeyNotFoundException("Sınıf bulunamadı.");

        var teacher = classroom.Teachers.FirstOrDefault(t => t.Id == request.TeacherId);
        if (teacher == null)
            throw new KeyNotFoundException("Bu öğretmen bu sınıfa atanmamış.");

        classroom.RemoveTeacher(teacher);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
