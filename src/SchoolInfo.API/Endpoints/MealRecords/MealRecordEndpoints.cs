using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SchoolInfo.Application.Features.MealRecords.Commands.UpdateMealRecord;

namespace SchoolInfo.API.Endpoints.MealRecords;

/// <summary>
/// Ã–ÄŸÃ¼n kayÄ±t iÅŸlemleri iÃ§in Minimal API endpoint'leri.
/// </summary>
public class MealRecordEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/meal-records").WithTags("Meal Records").RequireAuthorization();

        group.MapPut("/{studentId:guid}", UpdateMealRecordAsync)
            .WithName("UpdateMealRecord")
            .WithSummary("Ã–ÄŸrencinin Ã¶ÄŸÃ¼n kaydÄ±nÄ± gÃ¼nceller.");
    }

    private static async Task<IResult> UpdateMealRecordAsync(Guid studentId, UpdateMealRecordCommand command, IMediator mediator)
    {
        // Not: GerÃ§ek senaryoda StudentId Ã¼zerinden MealRecordId bulunur, Ã¶rnek olarak direkt gÃ¶nderiliyor.
        await mediator.Send(command);
        return Results.NoContent();
    }
}
