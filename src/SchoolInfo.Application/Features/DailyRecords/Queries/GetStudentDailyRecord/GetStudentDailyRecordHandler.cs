using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Application.Features.DailyRecords.Queries.GetStudentDailyRecord;

/// <summary>
/// Günlük kaydı getirme işlemini yürüten sınıf.
/// </summary>
public class GetStudentDailyRecordHandler : IRequestHandler<GetStudentDailyRecordQuery, DailyRecordDto?>
{
    private readonly IDailyRecordRepository _dailyRecordRepository;

    public GetStudentDailyRecordHandler(IDailyRecordRepository dailyRecordRepository)
    {
        _dailyRecordRepository = dailyRecordRepository;
    }

    public async Task<DailyRecordDto?> Handle(GetStudentDailyRecordQuery request, CancellationToken cancellationToken)
    {
        var record = await _dailyRecordRepository.GetByStudentAndDateAsync(request.StudentId, request.Date);
        if (record == null)
            return null;

        return new DailyRecordDto(
            record.Id,
            record.StudentId,
            record.Date,
            record.SleepInfo.Status.ToString(),
            record.WaterConsumption.AmountInMilliliters,
            record.TeacherNote
        );
    }
}
