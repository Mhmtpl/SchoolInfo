using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Schools.Commands.UpdateSchool;

public record UpdateSchoolCommand(Guid Id, string Name) : IRequest<bool>;

public class UpdateSchoolCommandHandler : IRequestHandler<UpdateSchoolCommand, bool>
{
    private readonly IAppDbContext _dbContext;

    public UpdateSchoolCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateSchoolCommand request, CancellationToken cancellationToken)
    {
        var school = await _dbContext.Schools.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (school == null)
            return false;

        school.UpdateName(request.Name);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
