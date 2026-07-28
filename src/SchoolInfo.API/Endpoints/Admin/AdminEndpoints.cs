using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using SchoolInfo.Application.Features.Admin.Queries.GetDashboardStats;
using SchoolInfo.Application.Features.Schools.Commands.CreateSchool;
using SchoolInfo.Application.Features.Schools.Commands.UpdateSchool;
using SchoolInfo.Application.Features.Schools.Commands.DeleteSchool;
using SchoolInfo.Application.Features.Schools.Queries.GetSchools;
using SchoolInfo.Application.Features.Schools.Queries.GetSchool;

namespace SchoolInfo.API.Endpoints.Admin;

public class AdminEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("/dashboard-stats", async (IMediator mediator) =>
        {
            var query = new GetDashboardStatsQuery();
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetDashboardStats")
        .WithSummary("Yönetici paneli için genel istatistikleri getirir.");

        // --- School Management Endpoints ---

        group.MapGet("/schools", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSchoolsQuery());
            return Results.Ok(result);
        })
        .WithName("GetSchools")
        .WithSummary("Tüm okulları listeler.");

        group.MapGet("/schools/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetSchoolQuery(id));
                return Results.Ok(result);
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("GetSchoolById")
        .WithSummary("Belirli bir okulu getirir.");

        group.MapPost("/schools", async ([FromBody] CreateSchoolCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(new { Id = result });
        })
        .WithName("CreateSchool")
        .WithSummary("Yeni bir okul oluşturur.");

        group.MapPut("/schools/{id:guid}", async (Guid id, [FromBody] UpdateSchoolCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return Results.BadRequest("Id mismatch");
            
            var result = await mediator.Send(command);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateSchool")
        .WithSummary("Bir okulu günceller.");

        group.MapDelete("/schools/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteSchoolCommand(id));
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteSchool")
        .WithSummary("Bir okulu siler.");

        // --- User Management Endpoints ---

        group.MapGet("/users", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new SchoolInfo.Application.Features.Users.Queries.GetUsers.GetUsersQuery());
            return Results.Ok(result);
        })
        .WithName("GetUsers")
        .WithSummary("Tüm kullanıcıları listeler.");

        group.MapPost("/users", async ([FromBody] SchoolInfo.Application.Features.Users.Commands.CreateUser.CreateUserCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(new { Id = result });
        })
        .WithName("CreateUser")
        .WithSummary("Yeni bir kullanıcı oluşturur.");

        group.MapPut("/users/{id:guid}", async (Guid id, [FromBody] SchoolInfo.Application.Features.Users.Commands.UpdateUser.UpdateUserCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return Results.BadRequest("Id mismatch");
            
            var result = await mediator.Send(command);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateUser")
        .WithSummary("Bir kullanıcıyı günceller.");

        group.MapDelete("/users/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SchoolInfo.Application.Features.Users.Commands.DeleteUser.DeleteUserCommand(id));
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteUser")
        .WithSummary("Bir kullanıcıyı siler.");
    }
}
