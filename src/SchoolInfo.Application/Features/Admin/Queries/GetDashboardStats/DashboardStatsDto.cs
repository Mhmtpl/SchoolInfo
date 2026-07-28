namespace SchoolInfo.Application.Features.Admin.Queries.GetDashboardStats;

public class DashboardStatsDto
{
    public int TotalSchools { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalParents { get; set; }
    public int ActiveEsp32Devices { get; set; }
    public int TodayAiSummaries { get; set; }
}
