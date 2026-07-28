using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Application.Features.Schools.Queries.GetSchool;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Schools.Queries.GetSchools;

public record GetSchoolsQuery : IRequest<List<SchoolDto>>;

public class GetSchoolsQueryHandler : IRequestHandler<GetSchoolsQuery, List<SchoolDto>>
{
    private readonly IAppDbContext _dbContext;

    public GetSchoolsQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SchoolDto>> Handle(GetSchoolsQuery request, CancellationToken cancellationToken)
    {
        var schools = await _dbContext.Schools
            .AsNoTracking()
            .Select(s => new SchoolDto(s.Id, s.Name))
            .ToListAsync(cancellationToken);

        return schools;
    }
}
