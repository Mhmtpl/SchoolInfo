using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Classrooms.Queries.GetClassroom;

public record GetClassroomQuery(Guid Id) : IRequest<ClassroomDto>;

public record ClassroomDto(Guid Id, string Name, Guid SchoolId);

public class GetClassroomHandler : IRequestHandler<GetClassroomQuery, ClassroomDto>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetClassroomHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ClassroomDto> Handle(GetClassroomQuery request, CancellationToken cancellationToken)
    {
        var classroom = await ((DbContext)_dbContext).Set<Classroom>()
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (classroom == null)
            throw new System.Collections.Generic.KeyNotFoundException("Sinif bulunamadi veya yetkiniz yok.");

        return new ClassroomDto(classroom.Id, classroom.Name, classroom.SchoolId);
    }
}
