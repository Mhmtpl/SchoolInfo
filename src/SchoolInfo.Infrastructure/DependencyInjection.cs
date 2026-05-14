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
/// Infrastructure katmanına ait bağımlılıkların IoC konteynerine eklendiği sınıf.
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
        services.AddScoped<IDailyRecordRepository, DailyRecordRepository>();
        services.AddScoped<IMealRecordRepository, MealRecordRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IDailySummaryRepository, DailySummaryRepository>();

        // Agent Framework Config
        var endpoint = configuration["AgentFramework:Endpoint"] ?? "https://example.com";
        var apiKey = configuration["AgentFramework:ApiKey"] ?? "key";
        var model = configuration["AgentFramework:Model"] ?? "gpt-4o";

        var instructions = @"Sen bir anaokulu ve ilkokul öğretmenisin.
Sana JSON formatında verilen günlük çocuk verilerini, ebeveyne hitap eden sıcak, güven verici ve pedagojik bir dille özetle.
Kurallar:
- Türkçe yaz
- 'Bugün [ÇocukAdı]...' diye başla
- Özbakım, beslenme ve öğrenme bilgilerini doğal akışta geç
- Negatif bilgileri (yemedi, uyumadı) yumuşat ama gizleme
- Maksimum 4 cümle
- Emoji kullanma
- Resmi değil, samimi dil kullan
- Sadece özet metni döndür, başka hiçbir şey yazma";

        // Agent instance'ı DI ile singleton olarak register et
        services.AddSingleton(provider => 
        {
            var client = new OpenAIClient(apiKey);
            var agent = new SchoolAIAgent(client, "SchoolSummaryAgent", instructions);
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
