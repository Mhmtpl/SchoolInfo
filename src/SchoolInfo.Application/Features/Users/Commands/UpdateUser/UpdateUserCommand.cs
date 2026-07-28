using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(Guid Id, string FirstName, string LastName, string Email, UserRole Role, Guid? SchoolId, string? Password) : IRequest<bool>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly IAppDbContext _dbContext;

    public UpdateUserCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null)
            return false;

        user.UpdateDetails(request.FirstName, request.LastName, request.Email);
        user.ChangeRole(request.Role);
        user.SchoolId = request.SchoolId ?? Guid.Empty;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
