using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Exceptions;
using SchoolInfo.Domain.Interfaces;
using SchoolInfo.Domain.ValueObjects;

namespace SchoolInfo.Application.Features.MealRecords.Commands.UpdateMealRecord;

/// <summary>
/// Öğün kaydı güncelleme işlemini yürüten sınıf.
/// </summary>
public class UpdateMealRecordCommandHandler : IRequestHandler<UpdateMealRecordCommand>
{
    private readonly IMealRecordRepository _mealRecordRepository;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMealRecordCommandHandler(
        IMealRecordRepository mealRecordRepository, 
        IAppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _mealRecordRepository = mealRecordRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateMealRecordCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Teacher" && _currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Öğün kaydı güncellemek için yetkiniz bulunmamaktadır.");
        }

        var mealRecord = await _mealRecordRepository.GetByIdAsync(request.MealRecordId);
        if (mealRecord == null)
        {
            throw new DomainException($"Id'si {request.MealRecordId} olan öğün kaydı bulunamadı.");
        }

        mealRecord.UpdateStatus(new MealStatus(request.StatusType, request.Description));

        await _mealRecordRepository.UpdateAsync(mealRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
