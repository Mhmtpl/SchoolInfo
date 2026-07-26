using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Students.Queries.GetParentStudents;

public class GetParentStudentsHandler : IRequestHandler<GetParentStudentsQuery, List<StudentDto>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetParentStudentsHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<StudentDto>> Handle(GetParentStudentsQuery request, CancellationToken cancellationToken)
    {
        var parent = await _dbContext.Users
            .Include(u => u.Students)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.ParentId && u.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (parent == null)
            return new List<StudentDto>();

        var list = new List<StudentDto>();
        foreach (var s in parent.Students)
        {
            var classroom = await ((DbContext)_dbContext).Set<Classroom>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == s.ClassroomId, cancellationToken);

            var school = await ((DbContext)_dbContext).Set<School>()
                .AsNoTracking()
                .FirstOrDefaultAsync(sc => sc.Id == s.SchoolId, cancellationToken);

            list.Add(new StudentDto(
                s.Id,
                s.FirstName,
                s.LastName,
                s.DateOfBirth,
                s.ClassroomId,
                classroom?.Name ?? "Bilinmeyen Sınıf",
                school?.Name ?? "Veliport Portal"
            ));
        }

        return list;
    }
}
