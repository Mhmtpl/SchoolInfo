using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Application.Features.DailyRecords.Queries.GetStudentDailyRecord;

/// <summary>
/// Günlük kaydı getirme işlemini yürüten sınıf.
/// </summary>
public class GetStudentDailyRecordHandler : IRequestHandler<GetStudentDailyRecordQuery, DailyRecordDto?>
{
    private readonly IDailyRecordRepository _dailyRecordRepository;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentDailyRecordHandler(
        IDailyRecordRepository dailyRecordRepository,
        IAppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dailyRecordRepository = dailyRecordRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<DailyRecordDto?> Handle(GetStudentDailyRecordQuery request, CancellationToken cancellationToken)
    {
        // Öğrenciyi veritabanından çek ve okul kontrolü yap
        var student = await ((DbContext)_dbContext).Set<Student>()
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && s.SchoolId == _currentUserService.SchoolId && !s.IsDeleted, cancellationToken);
        
        if (student == null)
        {
            throw new KeyNotFoundException("Öğrenci bulunamadı veya bu öğrenciye erişim yetkiniz yok.");
        }

        // Yetki Kontrolü
        if (_currentUserService.Role == "Parent")
        {
            var isMyChild = await ((DbContext)_dbContext).Set<Student>()
                .Where(s => s.Id == request.StudentId)
                .SelectMany(s => s.Parents)
                .AnyAsync(p => p.Id == _currentUserService.UserId, cancellationToken);

            if (!isMyChild)
                throw new UnauthorizedAccessException("Bu öğrencinin günlük kaydına erişim yetkiniz bulunmamaktadır.");
        }
        else if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await ((DbContext)_dbContext).Set<Classroom>()
                .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                .AnyAsync(c => c.Id == student.ClassroomId && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
                throw new UnauthorizedAccessException("Atanmadığınız bir sınıfın öğrencisinin günlük kaydına erişemezsiniz.");
        }

        var record = await _dailyRecordRepository.GetByStudentAndDateAsync(request.StudentId, request.Date);
        if (record == null)
            return null;

        return new DailyRecordDto(
            record.Id,
            record.StudentId,
            record.Date,
            record.SleepInfo.Status.ToString(),
            record.WaterConsumption.AmountInMilliliters,
            record.TeacherNote
        );
    }
}

