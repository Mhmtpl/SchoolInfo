using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Classrooms.Commands.CreateClassroom;

public record CreateClassroomCommand(string Name) : IRequest<Guid>;

public class CreateClassroomCommandHandler : IRequestHandler<CreateClassroomCommand, Guid>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateClassroomCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateClassroomCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Admin")
            throw new UnauthorizedAccessException("Sadece admin sinif olusturabilir.");

        var schoolId = _currentUserService.SchoolId;
        var classroom = new Classroom(request.Name, schoolId);
        
        await ((DbContext)_dbContext).Set<Classroom>().AddAsync(classroom, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return classroom.Id;
    }
}
