using System;
using System.Linq;
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

namespace SchoolInfo.Application.Features.DailyRecords.Commands.CreateDailyRecord;

/// <summary>
/// Günlük kayıt oluşturma işlemini yürüten sınıf.
/// </summary>
public class CreateDailyRecordCommandHandler : IRequestHandler<CreateDailyRecordCommand, Guid>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IDailyRecordRepository _dailyRecordRepository;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateDailyRecordCommandHandler(
        IStudentRepository studentRepository,
        IDailyRecordRepository dailyRecordRepository,
        IAppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _studentRepository = studentRepository;
        _dailyRecordRepository = dailyRecordRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateDailyRecordCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Teacher" && _currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Günlük kayıt oluşturmak için yetkiniz bulunmamaktadır.");
        }

        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
        {
            throw new StudentNotFoundException(request.StudentId);
        }

        var existingRecord = await _dailyRecordRepository.GetByStudentAndDateAsync(request.StudentId, request.Date);
        if (existingRecord != null)
        {
            throw new DomainException("Bu öğrenci için bu tarihte zaten bir günlük kayıt var.");
        }

        var dailyRecord = new DailyRecord(request.StudentId, request.Date);
        dailyRecord.SchoolId = student.SchoolId;
        
        await _dailyRecordRepository.AddAsync(dailyRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Haftalık yemek planı şablonlarını sorgulayalım
        var dayOfWeek = request.Date.DayOfWeek;
        var classroomId = student.ClassroomId;
        var weeklyPlans = await ((DbContext)_dbContext).Set<WeeklyMealPlan>()
            .Where(w => w.ClassroomId == classroomId && w.DayOfWeek == dayOfWeek && !w.IsDeleted)
            .ToListAsync(cancellationToken);

        var breakfastPlan = weeklyPlans.FirstOrDefault(w => w.MealName.Equals("Kahvaltı", StringComparison.OrdinalIgnoreCase));
        var lunchPlan = weeklyPlans.FirstOrDefault(w => w.MealName.Equals("Öğle Yemeği", StringComparison.OrdinalIgnoreCase));
        var snackPlan = weeklyPlans.FirstOrDefault(w => w.MealName.Equals("İkindi Kahvaltısı", StringComparison.OrdinalIgnoreCase));

        // Öğretmenlerin yemek durumunu güncelleyebilmesi için 3 öğün kaydını (Kahvaltı, Öğle Yemeği, İkindi Kahvaltısı) None olarak ilklendiriyoruz
        var kahvalti = new MealRecord(dailyRecord.Id, "Kahvaltı", new MealStatus(MealStatusType.None, string.Empty));
        kahvalti.SchoolId = student.SchoolId;
        if (breakfastPlan != null)
        {
            kahvalti.SetNutrition(breakfastPlan.PlannedCalories, breakfastPlan.FoodContent, breakfastPlan.ProteinGrams, breakfastPlan.CarbsGrams);
        }

        var ogleYemegi = new MealRecord(dailyRecord.Id, "Öğle Yemeği", new MealStatus(MealStatusType.None, string.Empty));
        ogleYemegi.SchoolId = student.SchoolId;
        if (lunchPlan != null)
        {
            ogleYemegi.SetNutrition(lunchPlan.PlannedCalories, lunchPlan.FoodContent, lunchPlan.ProteinGrams, lunchPlan.CarbsGrams);
        }

        var ikindi = new MealRecord(dailyRecord.Id, "İkindi Kahvaltısı", new MealStatus(MealStatusType.None, string.Empty));
        ikindi.SchoolId = student.SchoolId;
        if (snackPlan != null)
        {
            ikindi.SetNutrition(snackPlan.PlannedCalories, snackPlan.FoodContent, snackPlan.ProteinGrams, snackPlan.CarbsGrams);
        }

        var dbSet = ((DbContext)_dbContext).Set<MealRecord>();
        await dbSet.AddRangeAsync(new[] { kahvalti, ogleYemegi, ikindi }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return dailyRecord.Id;
    }
}

