using System.Threading.Tasks;
using SchoolInfo.Application.Features.DailySummary.Commands.GenerateDailySummary;

namespace SchoolInfo.Application.Common.Interfaces;

/// <summary>
/// Günlük kayıt verilerinden AI destekli özet üreten servis.
/// </summary>
public interface IAISummaryService
{
    Task<string> GenerateAsync(SummaryRequestDto request);
}
