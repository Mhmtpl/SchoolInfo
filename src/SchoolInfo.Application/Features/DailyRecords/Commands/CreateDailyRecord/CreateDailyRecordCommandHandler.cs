using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Exceptions;
using SchoolInfo.Domain.Interfaces;

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
        
        await _dailyRecordRepository.AddAsync(dailyRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return dailyRecord.Id;
    }
}
