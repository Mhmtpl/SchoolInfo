using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Application.Features.Biometrics.Queries.GetStudentBiometrics;

public record GetStudentBiometricsQuery(Guid StudentId, DateTime Date) : IRequest<List<StudentBiometricDto>>;

public record StudentBiometricDto(Guid Id, int? HeartRate, double? SpO2, double? BodyTemperature, DateTime RecordedAt);

public class GetStudentBiometricsQueryHandler : IRequestHandler<GetStudentBiometricsQuery, List<StudentBiometricDto>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentBiometricsQueryHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<StudentBiometricDto>> Handle(GetStudentBiometricsQuery request, CancellationToken cancellationToken)
    {
        // 1. Önce öğrencinin varlığını kontrol et ve Tenant doğrulaması yap
        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && s.SchoolId == _currentUserService.SchoolId && !s.IsDeleted, cancellationToken);

        if (student == null)
        {
            throw new KeyNotFoundException("Öğrenci bulunamadı.");
        }

        // 2. Yetki Kontrolleri (BOLA Önleme)
        if (_currentUserService.Role == "Parent")
        {
            var isParentOfStudent = await _dbContext.Students
                .AnyAsync(s => s.Id == request.StudentId && !s.IsDeleted && s.Parents.Any(p => p.Id == _currentUserService.UserId), cancellationToken);

            if (!isParentOfStudent)
            {
                throw new UnauthorizedAccessException("Bu öğrencinin sağlık verilerine erişim yetkiniz yok.");
            }
        }
        else if (_currentUserService.Role == "Teacher")
        {
            var isAssignedToClass = await _dbContext.Classrooms
                .AnyAsync(c => c.Id == student.ClassroomId && !c.IsDeleted && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssignedToClass)
            {
                throw new UnauthorizedAccessException("Sorumluluğunuzda olmayan bir sınıfın öğrencisinin verilerine erişemezsiniz.");
            }
        }
        else if (_currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");
        }

        // 3. Verilen tarihteki biyometrik kayıtları getir
        var startDate = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc);
        var endDate = startDate.AddDays(1);

        var records = await _dbContext.StudentBiometricRecords
            .Where(r => r.StudentId == request.StudentId && 
                        r.SchoolId == _currentUserService.SchoolId &&
                        r.RecordedAt >= startDate && 
                        r.RecordedAt < endDate && 
                        !r.IsDeleted)
            .OrderBy(r => r.RecordedAt)
            .Select(r => new StudentBiometricDto(
                r.Id,
                r.HeartRate,
                r.SpO2,
                r.BodyTemperature,
                r.RecordedAt
            ))
            .ToListAsync(cancellationToken);

        return records;
    }
}
