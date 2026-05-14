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
/// Aktivite iÅŸlemleri iÃ§in Minimal API endpoint'leri.
/// </summary>
public class ActivityEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/activities").WithTags("Activities").RequireAuthorization();

        group.MapPost("/", CreateActivityAsync)
            .WithName("CreateActivity")
            .WithSummary("Yeni aktivite oluÅŸturur.");

        group.MapPut("/{id:guid}/complete", CompleteActivityAsync)
            .WithName("CompleteActivity")
            .WithSummary("Belirtilen aktiviteyi tamamlandÄ± olarak iÅŸaretler.");
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
}
