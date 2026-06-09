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

namespace SchoolInfo.Application.Features.MedicationRecords.Commands.CreateMedicationRecord;

public class CreateMedicationRecordCommandHandler : IRequestHandler<CreateMedicationRecordCommand, Guid>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IDailyRecordRepository _dailyRecordRepository;
    private readonly IMedicationRecordRepository _medicationRecordRepository;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateMedicationRecordCommandHandler(
        IStudentRepository studentRepository,
        IDailyRecordRepository dailyRecordRepository,
        IMedicationRecordRepository medicationRecordRepository,
        IAppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _studentRepository = studentRepository;
        _dailyRecordRepository = dailyRecordRepository;
        _medicationRecordRepository = medicationRecordRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateMedicationRecordCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Teacher" && _currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("İlaç kaydı oluşturmak için yetkiniz bulunmamaktadır.");
        }

        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
        {
            throw new StudentNotFoundException(request.StudentId);
        }

        var today = DateTime.UtcNow.Date;
        var dailyRecord = await _dailyRecordRepository.GetByStudentAndDateAsync(request.StudentId, today);

        if (dailyRecord == null)
        {
            dailyRecord = new DailyRecord(request.StudentId, today)
            {
                SchoolId = student.SchoolId
            };
            await _dailyRecordRepository.AddAsync(dailyRecord);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Yemek kayıtlarını da ilklendirelim (CreateDailyRecordCommand ile aynı mantıkta)
            var dayOfWeek = today.DayOfWeek;
            var classroomId = student.ClassroomId;
            var weeklyPlans = await ((DbContext)_dbContext).Set<WeeklyMealPlan>()
                .Where(w => w.ClassroomId == classroomId && w.DayOfWeek == dayOfWeek && !w.IsDeleted)
                .ToListAsync(cancellationToken);

            var breakfastPlan = weeklyPlans.FirstOrDefault(w => w.MealName.Equals("Kahvaltı", StringComparison.OrdinalIgnoreCase));
            var lunchPlan = weeklyPlans.FirstOrDefault(w => w.MealName.Equals("Öğle Yemeği", StringComparison.OrdinalIgnoreCase));
            var snackPlan = weeklyPlans.FirstOrDefault(w => w.MealName.Equals("İkindi Kahvaltısı", StringComparison.OrdinalIgnoreCase));

            var kahvalti = new MealRecord(dailyRecord.Id, "Kahvaltı", new MealStatus(MealStatusType.None, string.Empty)) { SchoolId = student.SchoolId };
            if (breakfastPlan != null)
                kahvalti.SetNutrition(breakfastPlan.PlannedCalories, breakfastPlan.FoodContent, breakfastPlan.ProteinGrams, breakfastPlan.CarbsGrams);

            var ogleYemegi = new MealRecord(dailyRecord.Id, "Öğle Yemeği", new MealStatus(MealStatusType.None, string.Empty)) { SchoolId = student.SchoolId };
            if (lunchPlan != null)
                ogleYemegi.SetNutrition(lunchPlan.PlannedCalories, lunchPlan.FoodContent, lunchPlan.ProteinGrams, lunchPlan.CarbsGrams);

            var ikindi = new MealRecord(dailyRecord.Id, "İkindi Kahvaltısı", new MealStatus(MealStatusType.None, string.Empty)) { SchoolId = student.SchoolId };
            if (snackPlan != null)
                ikindi.SetNutrition(snackPlan.PlannedCalories, snackPlan.FoodContent, snackPlan.ProteinGrams, snackPlan.CarbsGrams);

            var dbSet = ((DbContext)_dbContext).Set<MealRecord>();
            await dbSet.AddRangeAsync(new[] { kahvalti, ogleYemegi, ikindi }, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var medicationRecord = new MedicationRecord(
            dailyRecord.Id,
            request.StudentId,
            request.MedicineName,
            request.Dosage,
            request.AdministrationTime,
            request.Taken,
            request.Note)
        {
            SchoolId = student.SchoolId
        };

        await _medicationRecordRepository.AddAsync(medicationRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return medicationRecord.Id;
    }
}
