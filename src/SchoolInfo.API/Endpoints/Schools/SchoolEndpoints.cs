using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using SchoolInfo.Application.Features.Schools.Commands.CreateSchool;
using SchoolInfo.Application.Features.Schools.Queries.GetSchool;

namespace SchoolInfo.API.Endpoints.Schools;

public class SchoolEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/schools").WithTags("Schools").RequireAuthorization();

        group.MapPost("/", async (CreateSchoolCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Ok(new { Id = id });
        })
        .WithName("CreateSchool")
        .WithSummary("Yeni okul oluşturur.")
        .WithDescription("Sadece Admin rolündeki kullanıcı okul oluşturabilir.");

        group.MapGet("/{id:guid}", async (System.Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSchoolQuery(id));
            return Results.Ok(result);
        })
        .WithName("GetSchool")
        .WithSummary("Belirtilen ID'ye sahip okulu getirir.")
        .WithDescription("Okul bilgilerini (Id, Name) döner.");
    }
}
