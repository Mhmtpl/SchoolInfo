using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Users.Queries.GetCurrentUser;

public record GetCurrentUserQuery() : IRequest<UserDto>;

public record UserDto(Guid Id, string FirstName, string LastName, string Email, string Role);

public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await ((DbContext)_dbContext).Set<User>()
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

        if (user == null)
            throw new System.Collections.Generic.KeyNotFoundException("Kullanici bulunamadi.");

        return new UserDto(user.Id, user.FirstName, user.LastName, user.Email, user.Role.ToString());
    }
}
