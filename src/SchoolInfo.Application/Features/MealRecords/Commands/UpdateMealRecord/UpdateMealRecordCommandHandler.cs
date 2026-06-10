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
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
