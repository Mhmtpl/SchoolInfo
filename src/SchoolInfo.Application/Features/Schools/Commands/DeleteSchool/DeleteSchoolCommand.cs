using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Schools.Commands.DeleteSchool;

public record DeleteSchoolCommand(Guid Id) : IRequest<bool>;

public class DeleteSchoolCommandHandler : IRequestHandler<DeleteSchoolCommand, bool>
{
    private readonly IAppDbContext _dbContext;

    public DeleteSchoolCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteSchoolCommand request, CancellationToken cancellationToken)
    {
        var school = await _dbContext.Schools.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (school == null)
            return false;

        _dbContext.Schools.Remove(school);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
