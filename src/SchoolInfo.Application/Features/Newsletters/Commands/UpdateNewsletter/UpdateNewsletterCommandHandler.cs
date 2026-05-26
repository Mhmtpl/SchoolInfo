using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Newsletters.Commands.UpdateNewsletter;

public class UpdateNewsletterCommandHandler : IRequestHandler<UpdateNewsletterCommand, bool>
{
    private readonly IAppDbContext _context;

    public UpdateNewsletterCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateNewsletterCommand request, CancellationToken cancellationToken)
    {
        var newsletter = await _context.Newsletters
            .Include(n => n.Sections)
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        if (newsletter == null)
            throw new Exception("Bülten bulunamadı.");

        if (newsletter.Status != SchoolInfo.Domain.Enums.NewsletterStatus.Draft)
            throw new Exception("Sadece taslak halindeki bültenler düzenlenebilir.");

        newsletter.UpdateDraft(request.Title, request.Content, request.ImageUrl, request.WeekName);

        // Remove old sections
        _context.NewsletterSections.RemoveRange(newsletter.Sections);
        newsletter.ClearSections();

        // Add new sections
        if (request.Sections != null && request.Sections.Any())
        {
            foreach (var sec in request.Sections)
            {
                newsletter.AddSection(sec.Subject, sec.ThisWeekSummary, sec.NextWeekTopic, sec.InstructorName);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
