using System;
using MediatR;

namespace SchoolInfo.Application.Features.DailyRecords.Queries.GetStudentDailyRecord;

/// <summary>
/// Öğrencinin belirli bir tarihteki günlük kaydını sorgulama isteği.
/// </summary>
public record GetStudentDailyRecordQuery(Guid StudentId, DateTime Date) : IRequest<DailyRecordDto?>;
