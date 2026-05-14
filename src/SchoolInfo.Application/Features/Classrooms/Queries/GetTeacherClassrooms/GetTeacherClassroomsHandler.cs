using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Application.Features.Classrooms.Queries.GetTeacherClassrooms;

/// <summary>
/// Giriş yapan öğretmenin atandığı sınıfları getirir.
/// Bir öğretmen birden fazla sınıfa atanmış olabilir.
/// </summary>
public class GetTeacherClassroomsHandler : IRequestHandler<GetTeacherClassroomsQuery, List<TeacherClassroomDto>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetTeacherClassroomsHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<TeacherClassroomDto>> Handle(GetTeacherClassroomsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var schoolId = _currentUserService.SchoolId;

        var classrooms = await _dbContext.Classrooms
            .Include(c => c.Teachers)
            .Where(c => c.SchoolId == schoolId && c.Teachers.Any(t => t.Id == userId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return classrooms.Select(c => new TeacherClassroomDto(c.Id, c.Name)).ToList();
    }
}
