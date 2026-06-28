using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Students.Queries.GetStudent;

public record GetStudentQuery(Guid Id) : IRequest<StudentDto>;

public record StudentDto(Guid Id, string FirstName, string LastName, Guid ClassroomId);

public class GetStudentHandler : IRequestHandler<GetStudentQuery, StudentDto>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<StudentDto> Handle(GetStudentQuery request, CancellationToken cancellationToken)
    {
        var student = await ((DbContext)_dbContext).Set<Student>()
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (student == null)
            throw new System.Collections.Generic.KeyNotFoundException("Ogrenci bulunamadi.");

        // Veli yalnızca kendi çocuğuna erişebilir (IDOR koruması)
        if (_currentUserService.Role == "Parent")
        {
            // Shadow join table üzerinden kontrol: StudentParents
            var isMyChild = await ((DbContext)_dbContext).Set<Student>()
                .Where(s => s.Id == request.Id)
                .SelectMany(s => s.Parents)
                .AnyAsync(p => p.Id == _currentUserService.UserId, cancellationToken);

            if (!isMyChild)
                throw new UnauthorizedAccessException("Bu öğrencinin bilgilerine erişim yetkiniz bulunmamaktadır.");
        }

        return new StudentDto(student.Id, student.FirstName, student.LastName, student.ClassroomId);
    }
}
