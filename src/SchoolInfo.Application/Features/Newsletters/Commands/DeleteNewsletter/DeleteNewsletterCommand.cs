using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolInfo.Application.Features.Newsletters.Commands.DeleteNewsletter;

public record DeleteNewsletterCommand(Guid Id) : IRequest<bool>;

public class DeleteNewsletterCommandHandler : IRequestHandler<DeleteNewsletterCommand, bool>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteNewsletterCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteNewsletterCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Admin" && _currentUserService.Role != "Teacher")
        {
            throw new UnauthorizedAccessException("Bülten silmek için yetkiniz bulunmamaktadır.");
        }

        var newsletter = await _dbContext.Newsletters
            .Include(n => n.Sections)
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (newsletter == null)
        {
            return false;
        }

        if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await _dbContext.Classrooms
                .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                .AnyAsync(c => c.Id == newsletter.ClassroomId && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Bu sınıfın bültenini silmek için yetkiniz bulunmamaktadır.");
            }
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

