using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Application.Features.Students.Queries.GetStudentParents;

public class GetStudentParentsHandler : IRequestHandler<GetStudentParentsQuery, List<ParentDto>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentParentsHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<ParentDto>> Handle(GetStudentParentsQuery request, CancellationToken cancellationToken)
    {
        var student = await _dbContext.Students
            .Include(s => s.Parents)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && s.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (student == null)
            return new List<ParentDto>();

        return student.Parents.Select(p => new ParentDto(p.Id, p.FirstName, p.LastName, p.Email)).ToList();
    }
}
