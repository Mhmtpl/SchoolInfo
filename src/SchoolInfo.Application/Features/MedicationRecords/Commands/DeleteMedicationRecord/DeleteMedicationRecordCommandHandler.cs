using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchoolInfo.Application.Common.Interfaces;
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

        medicationRecord.Delete();
        await _medicationRecordRepository.UpdateAsync(medicationRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
