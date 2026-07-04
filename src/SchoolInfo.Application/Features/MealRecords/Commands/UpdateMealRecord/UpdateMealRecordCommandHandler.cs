using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Enums;
using SchoolInfo.Domain.Exceptions;
using SchoolInfo.Domain.Interfaces;
using SchoolInfo.Domain.ValueObjects;

namespace SchoolInfo.Application.Features.MealRecords.Commands.UpdateMealRecord;

/// <summary>
/// Ã–ÄŸÃ¼n kaydÄ± gÃ¼ncelleme iÅŸlemini yÃ¼rÃ¼ten sÄ±nÄ±f.
/// </summary>
public class UpdateMealRecordCommandHandler : IRequestHandler<UpdateMealRecordCommand>
{
    private readonly IMealRecordRepository _mealRecordRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDailyRecordRepository _dailyRecordRepository;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMealRecordCommandHandler(
        IMealRecordRepository mealRecordRepository, 
        IStudentRepository studentRepository,
        IDailyRecordRepository dailyRecordRepository,
        IAppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _mealRecordRepository = mealRecordRepository;
        _studentRepository = studentRepository;
        _dailyRecordRepository = dailyRecordRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateMealRecordCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Teacher" && _currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Öğün kaydı güncellemek için yetkiniz bulunmamaktadır.");
        }

        var dbSet = ((DbContext)_dbContext).Set<MealRecord>();
        MealRecord? mealRecord = null;

        if (request.MealRecordId != Guid.Empty)
        {
            mealRecord = await _mealRecordRepository.GetByIdAsync(request.MealRecordId);
            if (mealRecord != null)
            {
                if (mealRecord.SchoolId != _currentUserService.SchoolId)
                {
                    throw new UnauthorizedAccessException("Bu öğün kaydını güncellemek için yetkiniz bulunmamaktadır.");
                }

                if (_currentUserService.Role == "Teacher")
                {
                    var dailyRecord = await _dailyRecordRepository.GetByIdAsync(mealRecord.DailyRecordId);
                    if (dailyRecord == null)
                        throw new KeyNotFoundException("İlişkili günlük kayıt bulunamadı.");

                    var isAssigned = await ((DbContext)_dbContext).Set<Classroom>()
                        .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                        .AnyAsync(c => c.Students.Any(s => s.Id == dailyRecord.StudentId) && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

                    if (!isAssigned)
                    {
                        throw new UnauthorizedAccessException("Bu öğrencinin öğün kaydını güncellemek için yetkiniz bulunmamaktadır.");
                    }
                }
            }
        }
        else if (request.StudentId.HasValue && !string.IsNullOrEmpty(request.MealName))
        {
            var today = DateTime.UtcNow.AddHours(3).Date;
            var dailyRecord = await _dailyRecordRepository.GetByStudentAndDateAsync(request.StudentId.Value, today);
            var student = await _studentRepository.GetByIdAsync(request.StudentId.Value);
            
            if (student == null)
            {
                throw new StudentNotFoundException(request.StudentId.Value);
            }

            if (student.SchoolId != _currentUserService.SchoolId)
            {
                throw new UnauthorizedAccessException("Bu öğrencinin öğün kaydını güncellemek için yetkiniz bulunmamaktadır.");
            }

            if (_currentUserService.Role == "Teacher")
            {
                var isAssigned = await ((DbContext)_dbContext).Set<Classroom>()
                    .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                    .AnyAsync(c => c.Id == student.ClassroomId && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

                if (!isAssigned)
                {
                    throw new UnauthorizedAccessException("Bu öğrencinin öğün kaydını güncellemek için yetkiniz bulunmamaktadır.");
                }
            }

            if (dailyRecord == null)

            {
                dailyRecord = new DailyRecord(request.StudentId.Value, today)
                {
                    SchoolId = student.SchoolId
                };
                await _dailyRecordRepository.AddAsync(dailyRecord);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var reqMealName = request.MealName.ToLower();
            mealRecord = await dbSet
                .FirstOrDefaultAsync(m => m.DailyRecordId == dailyRecord.Id && 
                                          m.MealName.ToLower() == reqMealName && 
                                          !m.IsDeleted, cancellationToken);

            if (mealRecord == null)
            {
                // Haftalık yemek planı şablonlarını sorgulayalım
                var dayOfWeek = today.DayOfWeek;
                var classroomId = student.ClassroomId;
                var weeklyPlans = await ((DbContext)_dbContext).Set<WeeklyMealPlan>()
                    .Where(w => w.ClassroomId == classroomId && w.DayOfWeek == dayOfWeek && !w.IsDeleted)
                    .ToListAsync(cancellationToken);

                var plan = weeklyPlans.FirstOrDefault(w => w.MealName.Equals(request.MealName, StringComparison.OrdinalIgnoreCase));

                mealRecord = new MealRecord(dailyRecord.Id, request.MealName, new MealStatus(request.StatusType, request.Description ?? string.Empty))
                {
                    SchoolId = student.SchoolId
                };

                if (plan != null)
                {
                    mealRecord.SetNutrition(plan.PlannedCalories, plan.FoodContent, plan.ProteinGrams, plan.CarbsGrams);
                }

                await dbSet.AddAsync(mealRecord, cancellationToken);
                await ClearCachedSummaryAsync(dailyRecord.Id, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return;
            }
        }

        if (mealRecord == null)
        {
            throw new DomainException($"Belirtilen kriterlere uygun öğün kaydı bulunamadı veya oluşturulamadı.");
        }

        mealRecord.UpdateStatus(new MealStatus(request.StatusType, request.Description ?? string.Empty));
        await _mealRecordRepository.UpdateAsync(mealRecord);
        await ClearCachedSummaryAsync(mealRecord.DailyRecordId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ClearCachedSummaryAsync(Guid dailyRecordId, CancellationToken cancellationToken)
    {
        var dailyRecord = await _dailyRecordRepository.GetByIdAsync(dailyRecordId);
        if (dailyRecord != null)
        {
            var existingSummary = await _dbContext.DailySummaries
                .FirstOrDefaultAsync(s => s.StudentId == dailyRecord.StudentId && s.Date == dailyRecord.Date, cancellationToken);
            if (existingSummary != null)
            {
                _dbContext.DailySummaries.Remove(existingSummary);
            }
        }
    }
}
