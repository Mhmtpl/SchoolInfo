using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SchoolInfo.Application.Features.Newsletters.Commands.CreateNewsletter;
using SchoolInfo.Application.Features.Newsletters.Commands.PublishNewsletter;
using SchoolInfo.Application.Features.Newsletters.Commands.UpdateNewsletter;
using SchoolInfo.Application.Features.Newsletters.Queries.GetClassroomNewsletters;
using SchoolInfo.Application.Features.Newsletters.Queries.GetNewsletterPdf;

namespace SchoolInfo.API.Endpoints.Newsletters;

public class NewsletterEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/newsletters").WithTags("Newsletters").RequireAuthorization();

        group.MapPost("/", CreateNewsletterAsync)
            .WithName("CreateNewsletter")
            .WithSummary("Yeni bir bülten taslağı oluşturur.");

        group.MapPut("/{id:guid}/publish", PublishNewsletterAsync)
            .WithName("PublishNewsletter")
            .WithSummary("Taslak halindeki bülteni yayınlar.");

        group.MapPut("/{id:guid}", UpdateNewsletterAsync)
            .WithName("UpdateNewsletter")
            .WithSummary("Taslak bülteni düzenler.");

        group.MapGet("/classroom/{classroomId:guid}", GetClassroomNewslettersAsync)
            .WithName("GetClassroomNewsletters")
            .WithSummary("Sınıfa ait bültenleri listeler.");

        group.MapGet("/{id:guid}/pdf", GetNewsletterPdfAsync)
            .WithName("GetNewsletterPdf")
            .WithSummary("Bülteni PDF dosyası olarak indirir.");

        group.MapDelete("/{id:guid}", DeleteNewsletterAsync)
            .WithName("DeleteNewsletter")
            .WithSummary("Bülteni (taslak veya yayınlanmış) siler.");
    }

    private static async Task<IResult> CreateNewsletterAsync(CreateNewsletterCommand command, IMediator mediator)
    {
        var id = await mediator.Send(command);
        return Results.Created($"/api/newsletters/{id}", id);
    }

    private static async Task<IResult> PublishNewsletterAsync(Guid id, IMediator mediator)
    {
        await mediator.Send(new PublishNewsletterCommand(id));
        return Results.NoContent();
    }

    private static async Task<IResult> GetClassroomNewslettersAsync(Guid classroomId, IMediator mediator)
    {
        var result = await mediator.Send(new GetClassroomNewslettersQuery(classroomId));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetNewsletterPdfAsync(Guid id, IMediator mediator)
    {
        var pdfBytes = await mediator.Send(new GetNewsletterPdfQuery(id));
        return Results.File(pdfBytes, "application/pdf", $"bulten_{id}.pdf");
    }

    private static async Task<IResult> UpdateNewsletterAsync(Guid id, UpdateNewsletterCommand command, IMediator mediator)
    {
        command.Id = id;
        await mediator.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteNewsletterAsync(Guid id, IMediator mediator)
    {
        var success = await mediator.Send(new SchoolInfo.Application.Features.Newsletters.Commands.DeleteNewsletter.DeleteNewsletterCommand(id));
        return success ? Results.NoContent() : Results.NotFound();
    }
}

