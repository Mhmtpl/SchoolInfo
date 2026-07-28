using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<List<UserDto>>;

public record UserDto(Guid Id, string FirstName, string LastName, string Email, UserRole Role, Guid? SchoolId, string SchoolName);

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IAppDbContext _dbContext;

    public GetUsersQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var schools = await _dbContext.Schools.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);
        var usersList = await _dbContext.Users.AsNoTracking().ToListAsync(cancellationToken);

        var users = usersList.Select(u => new UserDto(
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            u.Role,
            u.SchoolId,
            u.SchoolId != Guid.Empty && schools.ContainsKey(u.SchoolId) ? schools[u.SchoolId] : ""
        )).ToList();

        return users;
    }
}
