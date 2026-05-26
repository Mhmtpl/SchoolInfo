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
        })
        .WithName("CreateStudent")
        .WithSummary("Yeni öğrenci oluşturur.")
        .WithDescription("Admin veya Öğretmen rolü gerektirir. Öğrenci bir sınıfa atanmak zorundadır.");

        group.MapGet("/{id:guid}", async (System.Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetStudentQuery(id));
            return Results.Ok(result);
        })
        .WithName("GetStudent")
        .WithSummary("Öğrenci bilgilerini getirir.")
        .WithDescription("Öğrencinin adı, soyadı ve sınıf bilgisini döner. Sadece aynı okuldaki kullanıcı erişebilir.");

        group.MapPut("/{id:guid}", async (System.Guid id, UpdateStudentCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return Results.BadRequest("Id mismatch");
            await mediator.Send(command);
            return Results.NoContent();
        })
        .WithName("UpdateStudent")
        .WithSummary("Öğrenci bilgilerini günceller.")
        .WithDescription("Öğrencinin adı, soyadı ve sınıf ataması güncellenebilir. Admin veya Öğretmen yapabilir.");

        group.MapDelete("/{id:guid}", async (System.Guid id, IMediator mediator) =>
        {
            await mediator.Send(new SchoolInfo.Application.Features.Students.Commands.DeleteStudent.DeleteStudentCommand(id));
            return Results.NoContent();
        })
        .WithName("DeleteStudent")
        .WithSummary("Öğrenciyi siler (soft delete).")
        .WithDescription("Öğrenci fiziksel olarak silinmez, IsDeleted=true olarak işaretlenir. Sadece Admin yapabilir.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapPost("/{studentId:guid}/parent/{parentId:guid}", async (System.Guid studentId, System.Guid parentId, IMediator mediator) =>
        {
            await mediator.Send(new SchoolInfo.Application.Features.Students.Commands.LinkParentToStudent.LinkParentToStudentCommand(studentId, parentId));
            return Results.Ok();
        })
        .WithName("LinkParentToStudent")
        .WithSummary("Öğrenciye veli atar.")
        .WithDescription("Bir öğrencinin birden fazla velisi olabilir (boşanmış aile vb.). Sadece Admin yapabilir.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapDelete("/{studentId:guid}/parent/{parentId:guid}", async (System.Guid studentId, System.Guid parentId, IMediator mediator) =>
        {
            await mediator.Send(new SchoolInfo.Application.Features.Students.Commands.UnlinkParentFromStudent.UnlinkParentFromStudentCommand(studentId, parentId));
            return Results.NoContent();
        })
        .WithName("UnlinkParentFromStudent")
        .WithSummary("Öğrenci ile veli arasındaki bağlantı kaldırılır.")
        .WithDescription("Öğrenci-Veli ilişkisini siler. Velinin hesabı silinmez. Sadece Admin yapabilir.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapGet("/{studentId:guid}/parents", async (System.Guid studentId, IMediator mediator) =>
        {
            var result = await mediator.Send(new SchoolInfo.Application.Features.Students.Queries.GetStudentParents.GetStudentParentsQuery(studentId));
            return Results.Ok(result);
        })
        .WithName("GetStudentParents")
        .WithSummary("Öğrencinin kayıtlı velilerini listeler.")
        .WithDescription("Öğrenciye atanmış tüm velilerin Id, ad, soyad ve email bilgilerini döner.");

        group.MapGet("/my-children", async (IMediator mediator, SchoolInfo.Application.Common.Interfaces.ICurrentUserService currentUserService) =>
        {
            var result = await mediator.Send(new SchoolInfo.Application.Features.Students.Queries.GetParentStudents.GetParentStudentsQuery(currentUserService.UserId));
            return Results.Ok(result);
        })
        .WithName("GetMyChildren")
        .WithSummary("Giriş yapan velinin çocuklarını listeler.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Parent.ToString()));
    }
}
