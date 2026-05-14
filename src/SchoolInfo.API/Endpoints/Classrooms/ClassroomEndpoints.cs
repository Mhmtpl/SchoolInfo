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

        group.MapGet("/teacher/my", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Queries.GetTeacherClassrooms.GetTeacherClassroomsQuery());
            return Results.Ok(result);
        }).RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Teacher.ToString()));

        group.MapGet("/{id:guid}/daily-records/today", async (System.Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomDailyRecords.GetClassroomDailyRecordsQuery(id));
            return Results.Ok(result);
        }).RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Teacher.ToString()));

        group.MapGet("/{id:guid}/meal-records/today", async (System.Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomMealRecords.GetClassroomMealRecordsQuery(id));
            return Results.Ok(result);
        }).RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Teacher.ToString()));

        group.MapDelete("/{id:guid}", async (System.Guid id, IMediator mediator) =>
        {
            await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Commands.DeleteClassroom.DeleteClassroomCommand(id));
            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        // Öğretmen atama / çıkarma (birden fazla öğretmen desteği)
        group.MapPost("/{classroomId:guid}/teacher/{teacherId:guid}", async (System.Guid classroomId, System.Guid teacherId, IMediator mediator) =>
        {
            await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Commands.AssignTeacher.AssignTeacherToClassroomCommand(classroomId, teacherId));
            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapDelete("/{classroomId:guid}/teacher/{teacherId:guid}", async (System.Guid classroomId, System.Guid teacherId, IMediator mediator) =>
        {
            await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Commands.RemoveTeacher.RemoveTeacherFromClassroomCommand(classroomId, teacherId));
            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));
    }
}
