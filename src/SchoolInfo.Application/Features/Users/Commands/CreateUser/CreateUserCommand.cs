using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(string FirstName, string LastName, string Email, string Password, UserRole Role, Guid? SchoolId) : IRequest<Guid>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IAppDbContext _dbContext;

    public CreateUserCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        var user = new User(request.FirstName, request.LastName, request.Email, request.Role)
        {
            PasswordHash = passwordHash,
            SchoolId = request.SchoolId ?? Guid.Empty
        };

        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
