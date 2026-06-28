using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Agents.AI;
using OpenAI;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Interfaces;
using SchoolInfo.Infrastructure.AI;
using SchoolInfo.Infrastructure.Auth;
using SchoolInfo.Infrastructure.Notifications;
using SchoolInfo.Infrastructure.Persistence;
using SchoolInfo.Infrastructure.Persistence.Repositories;
using SchoolInfo.Infrastructure.BackgroundServices;

namespace SchoolInfo.Infrastructure;

/// <summary>
/// Infrastructure katmanÄ±na ait baÄŸÄ±mlÄ±lÄ±klarÄ±n IoC konteynerine eklendiÄŸi sÄ±nÄ±f.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Repositories
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IClassroomRepository, ClassroomRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDailyRecordRepository, DailyRecordRepository>();
        services.AddScoped<IMealRecordRepository, MealRecordRepository>();
        services.AddScoped<IMedicationRecordRepository, MedicationRecordRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IDailySummaryRepository, DailySummaryRepository>();

        // Agent Framework Config
        var endpoint = configuration["AgentFramework:Endpoint"] ?? "https://example.com";
        var apiKey = configuration["AgentFramework:ApiKey"] ?? "key";
        var model = configuration["AgentFramework:Model"] ?? "gpt-4o";

        var instructions = @"Sen bir anaokulu ve ilkokul Ã¶ÄŸretmenisin.
Sana JSON formatÄ±nda verilen gÃ¼nlÃ¼k Ã§ocuk verilerini, ebeveyne hitap eden sÄ±cak, gÃ¼ven verici ve pedagojik bir dille Ã¶zetle.
Kurallar:
- TÃ¼rkÃ§e yaz
- 'BugÃ¼n [Ã‡ocukAdÄ±]...' diye baÅŸla
- Ã–zbakÄ±m, beslenme ve Ã¶ÄŸrenme bilgilerini doÄŸal akÄ±ÅŸta geÃ§
- Negatif bilgileri (yemedi, uyumadÄ±) yumuÅŸat ama gizleme
- Maksimum 4 cÃ¼mle
- Emoji kullanma
- Resmi deÄŸil, samimi dil kullan
- Sadece Ã¶zet metni dÃ¶ndÃ¼r, baÅŸka hiÃ§bir ÅŸey yazma";

        services.AddHttpClient();

        // Agent instance'ını DI ile singleton olarak register et
        services.AddSingleton(provider => 
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            var agent = new SchoolAIAgent(httpClient, apiKey, model, instructions);
            return agent;
        });

        // Services
        services.AddScoped<IAISummaryService, AgentSummaryService>();
        services.AddScoped<INotificationService, FirebaseNotificationService>();
        services.AddSingleton<JwtTokenService>();

        // Background Services
        services.AddHostedService<DailySummaryScheduler>();

        return services;
    }
}
