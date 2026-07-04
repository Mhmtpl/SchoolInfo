using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Exceptions;
using SchoolInfo.Domain.Interfaces;
using SchoolInfo.Domain.ValueObjects;

namespace SchoolInfo.Application.Features.DailyRecords.Commands.UpdateDailyRecord;

/// <summary>
/// GÃ¼nlÃ¼k kaydÄ± gÃ¼ncelleme iÅŸlemini yÃ¼rÃ¼ten sÄ±nÄ±f.
/// </summary>
public class UpdateDailyRecordCommandHandler : IRequestHandler<UpdateDailyRecordCommand>
{
    private readonly IDailyRecordRepository _dailyRecordRepository;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateDailyRecordCommandHandler(
        IDailyRecordRepository dailyRecordRepository, 
        IAppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dailyRecordRepository = dailyRecordRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateDailyRecordCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Teacher" && _currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Günlük kaydı güncellemek için yetkiniz bulunmamaktadır.");
        }

        var dailyRecord = await _dailyRecordRepository.GetByIdAsync(request.DailyRecordId);
        if (dailyRecord == null)
        {
            throw new DomainException($"Id'si {request.DailyRecordId} olan günlük kayıt bulunamadı.");
        }

        if (dailyRecord.SchoolId != _currentUserService.SchoolId)
        {
            throw new UnauthorizedAccessException("Bu günlük kaydını güncellemek için yetkiniz bulunmamaktadır.");
        }

        if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await ((DbContext)_dbContext).Set<SchoolInfo.Domain.Entities.Classroom>()
                .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                .AnyAsync(c => c.Students.Any(s => s.Id == dailyRecord.StudentId) && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Bu öğrencinin günlük kaydını güncellemek için yetkiniz bulunmamaktadır.");
            }
        }


        dailyRecord.UpdateSleepInfo(new SleepData(request.SleepStatus, request.SleepStartTime, request.SleepEndTime));
        dailyRecord.UpdateWaterConsumption(new WaterIntake(request.WaterAmountInMilliliters));
        
        if (request.TeacherNote != null)
        {
            dailyRecord.SetTeacherNote(request.TeacherNote);
        }

        if (request.IsAbsent.HasValue)
        {
            dailyRecord.SetAbsentStatus(request.IsAbsent.Value);
        }

        // Veli sayfayı açtığında yeni verilerle tekrar AI özeti üretilmesi için eski özeti siliyoruz
        var existingSummary = await _dbContext.DailySummaries
            .FirstOrDefaultAsync(s => s.StudentId == dailyRecord.StudentId && s.Date == dailyRecord.Date, cancellationToken);
        if (existingSummary != null)
        {
            _dbContext.DailySummaries.Remove(existingSummary);
        }

        await _dailyRecordRepository.UpdateAsync(dailyRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
