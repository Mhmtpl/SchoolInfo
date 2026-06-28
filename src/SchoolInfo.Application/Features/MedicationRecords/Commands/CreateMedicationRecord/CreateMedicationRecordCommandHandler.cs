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
        if (_currentUserService.Role != "Teacher" && _currentUserService.Role != "Admin" && _currentUserService.Role != "Parent")
        {
            throw new UnauthorizedAccessException("İlaç kaydı oluşturmak için yetkiniz bulunmamaktadır.");
        }

        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
        {
            throw new StudentNotFoundException(request.StudentId);
        }

        var targetDate = request.Date?.Date ?? DateTime.UtcNow.AddHours(3).Date;
        var dailyRecord = await _dailyRecordRepository.GetByStudentAndDateAsync(request.StudentId, targetDate);

        if (dailyRecord == null)
        {
            dailyRecord = new DailyRecord(request.StudentId, targetDate)
            {
                SchoolId = student.SchoolId
            };
            await _dailyRecordRepository.AddAsync(dailyRecord);
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
