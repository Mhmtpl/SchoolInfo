using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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
