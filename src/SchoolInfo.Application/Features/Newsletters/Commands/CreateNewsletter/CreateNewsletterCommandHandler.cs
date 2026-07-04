using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Newsletters.Commands.CreateNewsletter;

public class CreateNewsletterCommandHandler : IRequestHandler<CreateNewsletterCommand, Guid>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateNewsletterCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateNewsletterCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Admin" && _currentUserService.Role != "Teacher")
        {
            throw new UnauthorizedAccessException("Bülten oluşturmak için yetkiniz bulunmamaktadır.");
        }

        if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await _dbContext.Classrooms
                .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                .AnyAsync(c => c.Id == request.ClassroomId && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Bu sınıfa bülten oluşturmak için yetkiniz bulunmamaktadır.");
            }
        }
        else if (_currentUserService.Role == "Admin")
        {
            var classroomExists = await _dbContext.Classrooms
                .AnyAsync(c => c.Id == request.ClassroomId && c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted, cancellationToken);

            if (!classroomExists)
            {
                throw new UnauthorizedAccessException("Sınıf bulunamadı veya bu sınıfa erişim yetkiniz yok.");
            }
        }

        var newsletter = new Newsletter(
            request.Title,
            request.Content ?? "",
            request.ImageUrl ?? "",
            request.WeekName,
            request.ClassroomId);

        if (request.Sections != null)
        {
            foreach (var section in request.Sections)
            {
                newsletter.AddSection(section.Subject, section.ThisWeekSummary, section.NextWeekTopic, section.InstructorName);
            }
        }
        newsletter.SchoolId = _currentUserService.SchoolId;

        await _dbContext.Newsletters.AddAsync(newsletter, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newsletter.Id;
    }
}

