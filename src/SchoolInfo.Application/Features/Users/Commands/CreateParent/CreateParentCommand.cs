using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.Users.Commands.CreateParent;

public record CreateParentCommand(string FirstName, string LastName, string Email, string Password) : IRequest<Guid>;

public class CreateParentCommandHandler : IRequestHandler<CreateParentCommand, Guid>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateParentCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateParentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Admin" && _currentUserService.Role != "Teacher")
            throw new UnauthorizedAccessException("Yetkiniz yok.");

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(request.FirstName, request.LastName, request.Email, UserRole.Parent);
        user.PasswordHash = hash;
        user.SchoolId = _currentUserService.SchoolId;

        await ((DbContext)_dbContext).Set<User>().AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
