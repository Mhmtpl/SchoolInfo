using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.Admin.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IAppDbContext _context;

    public GetDashboardStatsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var totalSchools = await _context.Schools.CountAsync(cancellationToken);
        var totalStudents = await _context.Students.CountAsync(cancellationToken);
        var totalTeachers = await _context.Users.CountAsync(u => u.Role == UserRole.Teacher, cancellationToken);
        var totalParents = await _context.Users.CountAsync(u => u.Role == UserRole.Parent, cancellationToken);
        
        var today = DateTime.UtcNow.Date;
        var todayAiSummaries = await _context.DailySummaries
            .CountAsync(s => s.CreatedAt >= today, cancellationToken);

        // TODO: Update when Esp32Device entity is implemented
        var activeEsp32Devices = 0;

        return new DashboardStatsDto
        {
            TotalSchools = totalSchools,
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalParents = totalParents,
            ActiveEsp32Devices = activeEsp32Devices,
            TodayAiSummaries = todayAiSummaries
        };
    }
}
