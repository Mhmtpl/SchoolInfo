using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SchoolInfo.Application.Features.Activities.Commands.CompleteActivity;
using SchoolInfo.Application.Features.Activities.Commands.CreateActivity;

namespace SchoolInfo.API.Endpoints.Activities;

/// <summary>
/// Aktivite iÃ…Å¸lemleri iÃƒÂ§in Minimal API endpoint'leri.
/// </summary>
public class ActivityEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/activities").WithTags("Activities").RequireAuthorization();

        group.MapPost("/", CreateActivityAsync)
            .WithName("CreateActivity")
            .WithSummary("Yeni aktivite oluÃ…Å¸turur.");

        group.MapPut("/{id:guid}/complete", CompleteActivityAsync)
            .WithName("CompleteActivity")
            .WithSummary("Belirtilen aktiviteyi tamamlandÃ„Â± olarak iÃ…Å¸aretler.");

        group.MapDelete("/{id:guid}", DeleteActivityAsync)
            .WithName("DeleteActivity")
            .WithSummary("Aktiviteyi siler (sadece tamamlanmamÄ±ÅŸ).")
            .RequireAuthorization();
    }

    private static async Task<IResult> CreateActivityAsync(CreateActivityCommand command, IMediator mediator)
    {
        var id = await mediator.Send(command);
        return Results.Created($"/api/activities/{id}", id);
    }

    private static async Task<IResult> CompleteActivityAsync(Guid id, IMediator mediator)
    {
        var command = new CompleteActivityCommand(id);
        await mediator.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteActivityAsync(Guid id, IMediator mediator)
    {
        await mediator.Send(new SchoolInfo.Application.Features.Activities.Commands.DeleteActivity.DeleteActivityCommand(id));
        return Results.NoContent();
    }
}
