using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.Users.Commands.CreateTeacher;

public record CreateTeacherCommand(string FirstName, string LastName, string Email, string Password) : IRequest<Guid>;

public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, Guid>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateTeacherCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Admin")
            throw new UnauthorizedAccessException("Sadece admin ogretmen ekleyebilir.");

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(request.FirstName, request.LastName, request.Email, UserRole.Teacher);
        user.PasswordHash = hash;
        user.SchoolId = _currentUserService.SchoolId;

        await ((DbContext)_dbContext).Set<User>().AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
