using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using SchoolInfo.Application.Features.Students.Commands.CreateStudent;
using SchoolInfo.Application.Features.Students.Commands.UpdateStudent;
using SchoolInfo.Application.Features.Students.Queries.GetStudent;

namespace SchoolInfo.API.Endpoints.Students;

public class StudentEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/students").WithTags("Students").RequireAuthorization();

        group.MapPost("/", async (CreateStudentCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Ok(new { Id = id });
        });

        group.MapGet("/{id:guid}", async (System.Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetStudentQuery(id));
            return Results.Ok(result);
        });

        group.MapPut("/{id:guid}", async (System.Guid id, UpdateStudentCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return Results.BadRequest("Id mismatch");
            await mediator.Send(command);
            return Results.NoContent();
        });
    }
}
