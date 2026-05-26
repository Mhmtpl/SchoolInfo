using System;
using System.Collections.Generic;
using MediatR;
using SchoolInfo.Application.Features.Newsletters.Commands.CreateNewsletter;

namespace SchoolInfo.Application.Features.Newsletters.Commands.UpdateNewsletter;

public class UpdateNewsletterCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? WeekName { get; set; }
    public List<NewsletterSectionDto> Sections { get; set; } = new();
}
