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

        group.MapDelete("/{id:guid}", async (System.Guid id, IMediator mediator) =>
        {
            await mediator.Send(new SchoolInfo.Application.Features.Students.Commands.DeleteStudent.DeleteStudentCommand(id));
            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapPost("/{studentId:guid}/parent/{parentId:guid}", async (System.Guid studentId, System.Guid parentId, IMediator mediator) =>
        {
            await mediator.Send(new SchoolInfo.Application.Features.Students.Commands.LinkParentToStudent.LinkParentToStudentCommand(studentId, parentId));
            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapDelete("/{studentId:guid}/parent/{parentId:guid}", async (System.Guid studentId, System.Guid parentId, IMediator mediator) =>
        {
            await mediator.Send(new SchoolInfo.Application.Features.Students.Commands.UnlinkParentFromStudent.UnlinkParentFromStudentCommand(studentId, parentId));
            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapGet("/{studentId:guid}/parents", async (System.Guid studentId, IMediator mediator) =>
        {
            var result = await mediator.Send(new SchoolInfo.Application.Features.Students.Queries.GetStudentParents.GetStudentParentsQuery(studentId));
            return Results.Ok(result);
        });
    }
}
