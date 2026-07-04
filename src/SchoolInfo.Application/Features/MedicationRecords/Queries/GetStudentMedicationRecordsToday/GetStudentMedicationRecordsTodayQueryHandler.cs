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

namespace SchoolInfo.Application.Features.MedicationRecords.Queries.GetStudentMedicationRecordsToday;

public class GetStudentMedicationRecordsTodayQueryHandler : IRequestHandler<GetStudentMedicationRecordsTodayQuery, List<MedicationRecordDto>>
{
    private readonly IMedicationRecordRepository _medicationRecordRepository;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentMedicationRecordsTodayQueryHandler(
        IMedicationRecordRepository medicationRecordRepository,
        IAppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _medicationRecordRepository = medicationRecordRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<MedicationRecordDto>> Handle(GetStudentMedicationRecordsTodayQuery request, CancellationToken cancellationToken)
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
                throw new UnauthorizedAccessException("Bu öğrencinin ilaç kayıtlarına erişim yetkiniz bulunmamaktadır.");
        }
        else if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await ((DbContext)_dbContext).Set<Classroom>()
                .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                .AnyAsync(c => c.Id == student.ClassroomId && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
                throw new UnauthorizedAccessException("Atanmadığınız bir sınıfın öğrencisinin ilaç kayıtlarına erişemezsiniz.");
        }

        var today = request.Date.HasValue ? DateTime.SpecifyKind(request.Date.Value.Date, DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.UtcNow.AddHours(3).Date, DateTimeKind.Utc);
        var records = await _medicationRecordRepository.GetByStudentAndDateAsync(request.StudentId, today);

        return records.Select(m => new MedicationRecordDto(
            m.Id,
            m.DailyRecordId,
            m.StudentId,
            m.MedicineName,
            m.Dosage,
            m.AdministrationTime,
            m.Taken,
            m.Note
        )).ToList();
    }
}

