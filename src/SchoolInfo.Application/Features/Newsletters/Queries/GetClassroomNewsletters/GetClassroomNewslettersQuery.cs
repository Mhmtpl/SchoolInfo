using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Newsletters.Queries.GetClassroomNewsletters;

public record GetClassroomNewslettersQuery(Guid ClassroomId) : IRequest<List<NewsletterDto>>;

public record NewsletterSectionDto(string Subject, string ThisWeekSummary, string NextWeekTopic, string InstructorName);

public record NewsletterDto(
    Guid Id,
    string Title,
    string Content,
    string ImageUrl,
    string? WeekName,
    List<NewsletterSectionDto> Sections,
    SchoolInfo.Domain.Enums.NewsletterStatus Status,
    DateTime? PublishedAt,
    DateTime CreatedAt
);

public class GetClassroomNewslettersQueryHandler : IRequestHandler<GetClassroomNewslettersQuery, List<NewsletterDto>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetClassroomNewslettersQueryHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<NewsletterDto>> Handle(GetClassroomNewslettersQuery request, CancellationToken cancellationToken)
    {
        var isParent = _currentUserService.Role == "Parent";
        
        var query = _dbContext.Newsletters
            .Include(n => n.Sections)
            .AsNoTracking()
            .Where(n => n.ClassroomId == request.ClassroomId && n.SchoolId == _currentUserService.SchoolId);

        // Veliler sadece yayınlanmış olanları görebilir
        if (isParent)
        {
            query = query.Where(n => n.Status == SchoolInfo.Domain.Enums.NewsletterStatus.Published);
        }

        var newsletters = await query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NewsletterDto(
                n.Id,
                n.Title,
                n.Content,
                n.ImageUrl,
                n.WeekName,
                n.Sections.Select(s => new NewsletterSectionDto(s.Subject, s.ThisWeekSummary, s.NextWeekTopic, s.InstructorName)).ToList(),
                n.Status,
                n.PublishedAt,
                n.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return newsletters;
    }
}
