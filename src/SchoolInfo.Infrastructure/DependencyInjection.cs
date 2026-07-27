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
using SchoolInfo.Infrastructure.Biometrics;

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

        var instructions = @"Sen bir anaokulu ve ilkokul öğretmenisin.
Sana JSON formatında verilen günlük çocuk verilerini, ebeveyne hitap eden sıcak, güven verici ve pedagojik bir dille özetle.
Kurallar:
- Türkçe yaz
- 'Bugün [ÇocukAdı]...' diye başla
- Özbakım, beslenme ve öğrenme bilgilerini doğal akışta geç
- Eğer veride çocuğun biyometrik (Biometrics) verileri varsa, bu verileri o saatteki ders programı aktivitesiyle bağdaştırarak çocuğun gün içindeki hareketlilik, odaklanma veya uyku derinliği durumunu hikayeye doğal bir şekilde ekle.
- Negatif bilgileri (yemedi, uyumadı) yumuşat ama gizleme
- Maksimum 4 cümle
- Emoji kullanma
- Resmi değil, samimi dil kullan
- Sadece özet metni döndür, başka hiçbir şey yazma";

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
        services.AddScoped<IAIClassroomParser, AIClassroomParser>();
        services.AddScoped<INotificationService, FirebaseNotificationService>();
        services.AddSingleton<JwtTokenService>();

        // Background Services
        services.AddHostedService<DailySummaryScheduler>();

        // Biometric Services
        services.AddSingleton<IBiometricBackgroundQueue, BiometricBackgroundQueue>();
        services.AddHostedService<BiometricQueueProcessor>();

        return services;
    }
}
