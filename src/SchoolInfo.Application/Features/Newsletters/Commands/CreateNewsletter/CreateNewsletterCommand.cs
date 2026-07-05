using System;
using System.Collections.Generic;
using MediatR;

namespace SchoolInfo.Application.Features.Newsletters.Commands.CreateNewsletter;

public record NewsletterSectionDto(string Subject, string ThisWeekSummary, string NextWeekTopic, string InstructorName);

public record CreateNewsletterCommand(
    Guid ClassroomId,
    string Title,
    string Content,
    string? ImageUrl,
    string? WeekName,
    List<NewsletterSectionDto> Sections,
    string? Theme
) : IRequest<Guid>;
