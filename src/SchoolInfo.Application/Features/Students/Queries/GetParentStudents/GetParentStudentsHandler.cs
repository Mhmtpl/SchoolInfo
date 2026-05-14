using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;

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

        return parent.Students.Select(s => new StudentDto(s.Id, s.FirstName, s.LastName, s.DateOfBirth, s.ClassroomId)).ToList();
    }
}
