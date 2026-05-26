using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using SchoolInfo.Application.Features.ActivityTemplates.Queries.GetActivityTemplates;

namespace SchoolInfo.API.Endpoints.ActivityTemplates;

public class ActivityTemplateEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/activity-templates").WithTags("Activity Templates").RequireAuthorization();

        group.MapGet("/", GetActivityTemplatesAsync)
            .WithName("GetActivityTemplates")
            .WithSummary("Okuldaki etkinlik şablonlarını listeler.");
    }

    private static async Task<IResult> GetActivityTemplatesAsync([FromQuery] string? category, IMediator mediator)
    {
        var result = await mediator.Send(new GetActivityTemplatesQuery(category));
        return Results.Ok(result);
    }
}
