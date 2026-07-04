using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Exceptions;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Application.Features.MedicationRecords.Commands.UpdateMedicationRecord;

public class UpdateMedicationRecordCommandHandler : IRequestHandler<UpdateMedicationRecordCommand>
{
    private readonly IMedicationRecordRepository _medicationRecordRepository;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMedicationRecordCommandHandler(
        IMedicationRecordRepository medicationRecordRepository,
        IAppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _medicationRecordRepository = medicationRecordRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateMedicationRecordCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Teacher" && _currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("İlaç kaydını güncellemek için yetkiniz bulunmamaktadır.");
        }

        var medicationRecord = await _medicationRecordRepository.GetByIdAsync(request.Id);
        if (medicationRecord == null)
        {
            throw new KeyNotFoundException("İlaç kaydı bulunamadı.");
        }

        if (medicationRecord.SchoolId != _currentUserService.SchoolId)
        {
            throw new UnauthorizedAccessException("Bu kaydı güncellemek için yetkiniz bulunmamaktadır.");
        }

        if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await ((DbContext)_dbContext).Set<Classroom>()
                .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                .AnyAsync(c => c.Students.Any(s => s.Id == medicationRecord.StudentId) && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Bu öğrencinin ilaç kaydını güncellemek için yetkiniz bulunmamaktadır.");
            }
        }

        medicationRecord.UpdateDetails(
            request.MedicineName,
            request.Dosage,
            request.AdministrationTime,
            request.Taken,
            request.Note);

        await _medicationRecordRepository.UpdateAsync(medicationRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

