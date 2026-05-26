using MediatR;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SchoolInfo.Application.Features.Newsletters.Commands.DeleteNewsletter;

public record DeleteNewsletterCommand(Guid Id) : IRequest<bool>;

public class DeleteNewsletterCommandHandler : IRequestHandler<DeleteNewsletterCommand, bool>
{
    private readonly IAppDbContext _dbContext;

    public DeleteNewsletterCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteNewsletterCommand request, CancellationToken cancellationToken)
    {
        var newsletter = await _dbContext.Newsletters
            .Include(n => n.Sections)
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        if (newsletter == null)
        {
            return false;
        }

        newsletter.Delete();
        foreach (var section in newsletter.Sections)
        {
            section.Delete();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
