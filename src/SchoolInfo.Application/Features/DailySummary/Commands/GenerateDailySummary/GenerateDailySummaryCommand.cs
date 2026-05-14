using System;
using MediatR;

namespace SchoolInfo.Application.Features.DailySummary.Commands.GenerateDailySummary;

/// <summary>
/// Günlük özet raporu oluşturma isteği.
/// </summary>
public record GenerateDailySummaryCommand(Guid StudentId, DateTime Date) : IRequest<Guid>;
