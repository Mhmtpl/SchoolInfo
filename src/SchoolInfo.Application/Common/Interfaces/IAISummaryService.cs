using System.Threading.Tasks;
using SchoolInfo.Application.Features.DailySummary.Commands.GenerateDailySummary;

namespace SchoolInfo.Application.Common.Interfaces;

/// <summary>
/// GÃ¼nlÃ¼k kayÄ±t verilerinden AI destekli Ã¶zet Ã¼reten servis.
/// </summary>
public interface IAISummaryService
{
    Task<string> GenerateAsync(SummaryRequestDto request);
}
