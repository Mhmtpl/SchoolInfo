using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SchoolInfo.Application.Features.MealRecords.Commands.UpdateMealRecord;

namespace SchoolInfo.API.Endpoints.MealRecords;

/// <summary>
/// Öğün kayıt işlemleri için Minimal API endpoint'leri.
/// </summary>
public class MealRecordEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/meal-records").WithTags("Meal Records").RequireAuthorization();

        group.MapPut("/{studentId:guid}", UpdateMealRecordAsync)
            .WithName("UpdateMealRecord")
            .WithSummary("Öğrencinin öğün kaydını günceller.");
    }

    private static async Task<IResult> UpdateMealRecordAsync(Guid studentId, UpdateMealRecordCommand command, IMediator mediator)
    {
        if (command.StudentId == Guid.Empty || command.StudentId == null)
        {
            command = command with { StudentId = studentId };
        }
        await mediator.Send(command);
        return Results.NoContent();
    }
}
