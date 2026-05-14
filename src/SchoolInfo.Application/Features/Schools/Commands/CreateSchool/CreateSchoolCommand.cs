using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Schools.Commands.CreateSchool;

public record CreateSchoolCommand(string Name) : IRequest<Guid>;

public class CreateSchoolCommandHandler : IRequestHandler<CreateSchoolCommand, Guid>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateSchoolCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateSchoolCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Admin")
            throw new UnauthorizedAccessException("Sadece admin okul olusturabilir.");

        var school = new School(request.Name);
        
        // EF DbContext using
        await ((Microsoft.EntityFrameworkCore.DbContext)_dbContext).Set<School>().AddAsync(school, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return school.Id;
    }
}
