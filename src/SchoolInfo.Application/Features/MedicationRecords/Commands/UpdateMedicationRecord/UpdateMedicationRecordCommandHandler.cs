using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchoolInfo.Application.Common.Interfaces;
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
