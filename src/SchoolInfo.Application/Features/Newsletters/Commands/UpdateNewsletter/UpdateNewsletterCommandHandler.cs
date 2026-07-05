using System;
using System.Collections.Generic;
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
    private readonly ICurrentUserService _currentUserService;

    public UpdateNewsletterCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdateNewsletterCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Admin" && _currentUserService.Role != "Teacher")
        {
            throw new UnauthorizedAccessException("Bülten güncellemek için yetkiniz bulunmamaktadır.");
        }

        var newsletter = await _context.Newsletters
            .Include(n => n.Sections)
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (newsletter == null)
            throw new KeyNotFoundException("Bülten bulunamadı veya bu bültene erişim yetkiniz yok.");

        if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await _context.Classrooms
                .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                .AnyAsync(c => c.Id == newsletter.ClassroomId && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Bu sınıfın bültenini güncellemek için yetkiniz bulunmamaktadır.");
            }
        }

        if (newsletter.Status != SchoolInfo.Domain.Enums.NewsletterStatus.Draft)
            throw new Exception("Sadece taslak halindeki bültenler düzenlenebilir.");

        newsletter.UpdateDraft(request.Title, request.Content, request.ImageUrl, request.WeekName, request.Theme ?? "Default");

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

