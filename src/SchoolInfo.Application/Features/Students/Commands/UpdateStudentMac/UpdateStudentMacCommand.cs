using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Students.Commands.UpdateStudentMac;

public record UpdateStudentMacCommand(Guid Id, string? MacAddress) : IRequest;

public class UpdateStudentMacCommandHandler : IRequestHandler<UpdateStudentMacCommand>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateStudentMacCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateStudentMacCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Admin" && _currentUserService.Role != "Teacher")
            throw new UnauthorizedAccessException("Yetkiniz yok.");

        var student = await ((DbContext)_dbContext).Set<Student>()
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (student == null)
            throw new System.Collections.Generic.KeyNotFoundException("Öğrenci bulunamadı.");

        student.SetSmartBandMac(request.MacAddress);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
