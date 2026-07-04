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

namespace SchoolInfo.Application.Features.MedicationRecords.Commands.DeleteMedicationRecord;

public class DeleteMedicationRecordCommandHandler : IRequestHandler<DeleteMedicationRecordCommand>
{
    private readonly IMedicationRecordRepository _medicationRecordRepository;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteMedicationRecordCommandHandler(
        IMedicationRecordRepository medicationRecordRepository,
        IAppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _medicationRecordRepository = medicationRecordRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteMedicationRecordCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Teacher" && _currentUserService.Role != "Admin" && _currentUserService.Role != "Parent")
        {
            throw new UnauthorizedAccessException("İlaç kaydını silmek için yetkiniz bulunmamaktadır.");
        }

        var medicationRecord = await _medicationRecordRepository.GetByIdAsync(request.Id);
        if (medicationRecord == null)
        {
            throw new KeyNotFoundException("İlaç kaydı bulunamadı.");
        }

        if (medicationRecord.SchoolId != _currentUserService.SchoolId)
        {
            throw new UnauthorizedAccessException("Bu kaydı silmek için yetkiniz bulunmamaktadır.");
        }

        if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await ((DbContext)_dbContext).Set<Classroom>()
                .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                .AnyAsync(c => c.Students.Any(s => s.Id == medicationRecord.StudentId) && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Bu öğrencinin ilaç kaydını silmek için yetkiniz bulunmamaktadır.");
            }
        }
        else if (_currentUserService.Role == "Parent")
        {
            var isMyChild = await ((DbContext)_dbContext).Set<Student>()
                .Where(s => s.Id == medicationRecord.StudentId && !s.IsDeleted)
                .SelectMany(s => s.Parents)
                .AnyAsync(p => p.Id == _currentUserService.UserId, cancellationToken);

            if (!isMyChild)
            {
                throw new UnauthorizedAccessException("Bu öğrencinin ilaç kaydını silmek için yetkiniz bulunmamaktadır.");
            }
        }

        medicationRecord.Delete();
        await _medicationRecordRepository.UpdateAsync(medicationRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

