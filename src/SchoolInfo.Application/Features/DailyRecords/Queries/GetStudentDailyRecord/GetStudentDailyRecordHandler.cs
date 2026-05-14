using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Application.Features.DailyRecords.Queries.GetStudentDailyRecord;

/// <summary>
/// GÃ¼nlÃ¼k kaydÄ± getirme iÅŸlemini yÃ¼rÃ¼ten sÄ±nÄ±f.
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
