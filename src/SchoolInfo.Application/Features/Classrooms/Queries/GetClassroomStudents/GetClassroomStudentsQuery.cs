using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomStudents;

public record GetClassroomStudentsQuery(Guid ClassroomId) : IRequest<List<StudentDto>>;

public record StudentDto(Guid Id, string FirstName, string LastName);

public class GetClassroomStudentsHandler : IRequestHandler<GetClassroomStudentsQuery, List<StudentDto>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetClassroomStudentsHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<StudentDto>> Handle(GetClassroomStudentsQuery request, CancellationToken cancellationToken)
    {
        var classroom = await ((DbContext)_dbContext).Set<Classroom>()
            .FirstOrDefaultAsync(c => c.Id == request.ClassroomId && c.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (classroom == null)
            throw new System.Collections.Generic.KeyNotFoundException("Sinif bulunamadi veya yetkiniz yok.");

        var students = await ((DbContext)_dbContext).Set<Student>()
            .Where(s => s.ClassroomId == request.ClassroomId && s.SchoolId == _currentUserService.SchoolId)
            .Select(s => new StudentDto(s.Id, s.FirstName, s.LastName))
            .ToListAsync(cancellationToken);

        return students;
    }
}
