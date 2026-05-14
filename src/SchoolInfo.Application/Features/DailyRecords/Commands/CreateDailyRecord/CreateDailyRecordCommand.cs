using System;
using MediatR;

namespace SchoolInfo.Application.Features.DailyRecords.Commands.CreateDailyRecord;

/// <summary>
/// Günlük kayıt oluşturma isteği.
/// </summary>
public record CreateDailyRecordCommand(Guid StudentId, DateTime Date) : IRequest<Guid>;
