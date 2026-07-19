using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using SchoolInfo.Application.Features.Biometrics.Commands.SaveBiometricData;
using SchoolInfo.Application.Features.Biometrics.Queries.GetStudentBiometrics;

namespace SchoolInfo.API.Endpoints.IoT;

/// <summary>
/// IoT ve Biyometrik veri endpoint'lerini tanımlayan sınıf.
/// </summary>
public class IoTEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // ESP32'nin veri yollayacağı halka açık ama token korumalı IoT grubu
        var group = app.MapGroup("/api/iot").WithTags("IoT").AllowAnonymous();

        group.MapPost("/biometrics", SaveBiometricDataAsync)
            .WithName("SaveBiometricData")
            .WithSummary("ESP32 cihazından gelen canlı biyometrik verileri kuyruğa ekler.");

        // Veli ve öğretmenlerin kullanacağı yetkilendirilmiş API grubu
        var secureGroup = app.MapGroup("/api/students/{studentId:guid}/biometrics").WithTags("Biometrics").RequireAuthorization();

        secureGroup.MapGet("/", GetStudentBiometricsAsync)
            .WithName("GetStudentBiometrics")
            .WithSummary("Öğrencinin belirli bir tarihteki biyometrik geçmiş verilerini listeler.");
    }

    private static async Task<IResult> SaveBiometricDataAsync(
        SaveBiometricDataCommand command, 
        HttpContext context, 
        IConfiguration configuration, 
        IMediator mediator)
    {
        // Güvenlik doğrulaması: X-IoT-Device-Token başlığı kontrolü
        var expectedToken = configuration["IoT:DeviceToken"] ?? "DefaultSecretIoTToken1234!";
        if (!context.Request.Headers.TryGetValue("X-IoT-Device-Token", out var receivedToken) ||
            receivedToken.ToString() != expectedToken)
        {
            return Results.Unauthorized();
        }

        var success = await mediator.Send(command);
        
        // Cihaza hızlıca yanıt veriyoruz (202 Accepted)
        return success 
            ? Results.Accepted() 
            : Results.BadRequest(new { success = false, message = "Eşleşen aktif öğrenci bulunamadı." });
    }

    private static async Task<IResult> GetStudentBiometricsAsync(
        Guid studentId, 
        string? date, 
        IMediator mediator)
    {
        if (!DateTime.TryParse(date, out var queryDate))
        {
            queryDate = DateTime.UtcNow.AddHours(3).Date; // Default to Turkey Local Time Date
        }

        var result = await mediator.Send(new GetStudentBiometricsQuery(studentId, queryDate));
        return Results.Ok(result);
    }
}
