using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Schools.Queries.GetSchool;

public record GetSchoolQuery(Guid Id) : IRequest<SchoolDto>;

public record SchoolDto(Guid Id, string Name);

public class GetSchoolHandler : IRequestHandler<GetSchoolQuery, SchoolDto>
{
    private readonly IAppDbContext _dbContext;

    public GetSchoolHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SchoolDto> Handle(GetSchoolQuery request, CancellationToken cancellationToken)
    {
        var school = await ((DbContext)_dbContext).Set<School>()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (school == null)
            throw new System.Collections.Generic.KeyNotFoundException("Okul bulunamadi.");

        return new SchoolDto(school.Id, school.Name);
    }
}
