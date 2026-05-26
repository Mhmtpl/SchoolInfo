using System;
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
        var newsletter = await _dbContext.Newsletters
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (newsletter == null)
            throw new System.Collections.Generic.KeyNotFoundException("Bülten bulunamadı.");

        newsletter.Publish();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
