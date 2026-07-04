using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Newsletters.Commands.PublishNewsletter;

public class PublishNewsletterCommandHandler : IRequestHandler<PublishNewsletterCommand>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public PublishNewsletterCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(PublishNewsletterCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Admin" && _currentUserService.Role != "Teacher")
        {
            throw new UnauthorizedAccessException("Bülten yayınlamak için yetkiniz bulunmamaktadır.");
        }

        var newsletter = await _dbContext.Newsletters
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (newsletter == null)
            throw new System.Collections.Generic.KeyNotFoundException("Bülten bulunamadı veya bu bültene erişim yetkiniz yok.");

        if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await _dbContext.Classrooms
                .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                .AnyAsync(c => c.Id == newsletter.ClassroomId && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Bu sınıfın bültenini yayınlamak için yetkiniz bulunmamaktadır.");
            }
        }

        newsletter.Publish();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

