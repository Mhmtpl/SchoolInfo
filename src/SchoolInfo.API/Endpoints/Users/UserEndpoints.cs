using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using SchoolInfo.Application.Features.Users.Commands.CreateTeacher;
using SchoolInfo.Application.Features.Users.Commands.CreateParent;
using SchoolInfo.Application.Features.Users.Queries.GetCurrentUser;

namespace SchoolInfo.API.Endpoints.Users;

public class UserEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();

        group.MapPost("/teacher", async (CreateTeacherCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Ok(new { Id = id });
        });

        group.MapPost("/parent", async (CreateParentCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Ok(new { Id = id });
        });

        group.MapGet("/me", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCurrentUserQuery());
            return Results.Ok(result);
        });

        group.MapPut("/me", async (SchoolInfo.Application.Features.Users.Commands.UpdateCurrentUser.UpdateCurrentUserCommand command, IMediator mediator) =>
        {
            await mediator.Send(command);
            return Results.NoContent();
        });

        group.MapPut("/me/password", async (SchoolInfo.Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand command, IMediator mediator) =>
        {
            await mediator.Send(command);
            return Results.NoContent();
        });
    }
}
