using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Agents.AI;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Application.Features.DailySummary.Commands.GenerateDailySummary;

namespace SchoolInfo.Infrastructure.AI;

/// <summary>
/// Microsoft Agent Framework kullanarak günlük kayıt özeti üreten servis.
/// </summary>
public class AgentSummaryService : IAISummaryService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AgentSummaryService> _logger;
    private readonly SchoolAIAgent _agent;

    public AgentSummaryService(IConfiguration configuration, ILogger<AgentSummaryService> logger, SchoolAIAgent agent)
    {
        _configuration = configuration;
        _logger = logger;
        _agent = agent;
    }

    public async Task<string> GenerateAsync(SummaryRequestDto request)
    {
        try
        {
            var jsonData = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            
            // RunAsync ile AI özeti üret
            var response = await _agent.RunAsync(jsonData);

            return response?.ToString() ?? GetFallbackMessage(request.StudentName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yapay zeka özeti oluşturulurken hata meydana geldi. Fallback metin üretiliyor.");
            return GetFallbackMessage(request.StudentName);
        }
    }

    private string GetFallbackMessage(string studentName)
    {
        return $"{studentName} bugün okulda güzel vakit geçirdi. Detaylar için öğretmeniyle iletişime geçebilirsiniz.";
    }
}
