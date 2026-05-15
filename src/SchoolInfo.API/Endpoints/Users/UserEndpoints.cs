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
        })
        .WithName("CreateTeacher")
        .WithSummary("Yeni öğretmen oluşturur.")
        .WithDescription("Sadece Admin rolü öğretmen ekleyebilir.");

        group.MapPost("/parent", async (CreateParentCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Ok(new { Id = id });
        })
        .WithName("CreateParent")
        .WithSummary("Yeni veli oluşturur.")
        .WithDescription("Sadece Admin rolü veli ekleyebilir.");

        group.MapGet("/me", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCurrentUserQuery());
            return Results.Ok(result);
        })
        .WithName("GetCurrentUser")
        .WithSummary("Mevcut kullanıcı bilgilerini getirir.")
        .WithDescription("Giriş yapmış olan kullanıcının profil bilgilerini döner.");

        group.MapPut("/me", async (SchoolInfo.Application.Features.Users.Commands.UpdateCurrentUser.UpdateCurrentUserCommand command, IMediator mediator) =>
        {
            await mediator.Send(command);
            return Results.NoContent();
        })
        .WithName("UpdateCurrentUser")
        .WithSummary("Mevcut kullanıcı profilini günceller.")
        .WithDescription("Kullanıcının adı, soyadı ve iletişim bilgilerini günceller.");

        group.MapPut("/me/password", async (SchoolInfo.Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand command, IMediator mediator) =>
        {
            await mediator.Send(command);
            return Results.NoContent();
        })
        .WithName("ChangePassword")
        .WithSummary("Şifre değiştirir.")
        .WithDescription("Kullanıcının mevcut şifresini doğrulayarak yeni şifre belirlemesini sağlar.");
    }
}
