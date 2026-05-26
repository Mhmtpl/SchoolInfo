using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.ActivityTemplates.Queries.GetActivityTemplates;

public record GetActivityTemplatesQuery(string? Category = null) : IRequest<List<ActivityTemplateDto>>;

public record ActivityTemplateDto(
    Guid Id,
    string Title,
    string Description,
    string Category,
    string RequiredMaterials,
    string AgeGroup
);

public class GetActivityTemplatesQueryHandler : IRequestHandler<GetActivityTemplatesQuery, List<ActivityTemplateDto>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetActivityTemplatesQueryHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<ActivityTemplateDto>> Handle(GetActivityTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ActivityTemplates
            .AsNoTracking()
            .Where(t => t.SchoolId == _currentUserService.SchoolId);

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(t => t.Category == request.Category);
        }

        var templates = await query
            .OrderBy(t => t.Title)
            .Select(t => new ActivityTemplateDto(
                t.Id,
                t.Title,
                t.Description,
                t.Category,
                t.RequiredMaterials,
                t.AgeGroup
            ))
            .ToListAsync(cancellationToken);

        return templates;
    }
}
