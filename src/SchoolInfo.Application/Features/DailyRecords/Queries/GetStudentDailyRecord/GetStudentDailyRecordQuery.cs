using System;
using MediatR;

namespace SchoolInfo.Application.Features.DailyRecords.Queries.GetStudentDailyRecord;

/// <summary>
/// Ã–ÄŸrencinin belirli bir tarihteki gÃ¼nlÃ¼k kaydÄ±nÄ± sorgulama isteÄŸi.
/// </summary>
public record GetStudentDailyRecordQuery(Guid StudentId, DateTime Date) : IRequest<DailyRecordDto?>;
