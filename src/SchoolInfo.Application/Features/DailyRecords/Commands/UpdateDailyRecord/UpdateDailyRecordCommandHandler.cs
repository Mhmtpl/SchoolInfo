using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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
            throw new UnauthorizedAccessException("GÃ¼nlÃ¼k kaydÄ± gÃ¼ncellemek iÃ§in yetkiniz bulunmamaktadÄ±r.");
        }

        var dailyRecord = await _dailyRecordRepository.GetByIdAsync(request.DailyRecordId);
        if (dailyRecord == null)
        {
            throw new DomainException($"Id'si {request.DailyRecordId} olan gÃ¼nlÃ¼k kayÄ±t bulunamadÄ±.");
        }

        dailyRecord.UpdateSleepInfo(new SleepData(request.SleepStatus, request.SleepStartTime, request.SleepEndTime));
        dailyRecord.UpdateWaterConsumption(new WaterIntake(request.WaterAmountInMilliliters));
        
        if (request.TeacherNote != null)
        {
            dailyRecord.SetTeacherNote(request.TeacherNote);
        }

        await _dailyRecordRepository.UpdateAsync(dailyRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
