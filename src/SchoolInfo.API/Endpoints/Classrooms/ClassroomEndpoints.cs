using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using SchoolInfo.Application.Features.Classrooms.Commands.CreateClassroom;
using SchoolInfo.Application.Features.Classrooms.Queries.GetClassroom;
using SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomStudents;

namespace SchoolInfo.API.Endpoints.Classrooms;

public class ClassroomEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/classrooms").WithTags("Classrooms").RequireAuthorization();

        group.MapPost("/", async (CreateClassroomCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Ok(new { Id = id });
        });

        group.MapGet("/{id:guid}", async (System.Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetClassroomQuery(id));
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}/students", async (System.Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetClassroomStudentsQuery(id));
            return Results.Ok(result);
        });
    }
}
