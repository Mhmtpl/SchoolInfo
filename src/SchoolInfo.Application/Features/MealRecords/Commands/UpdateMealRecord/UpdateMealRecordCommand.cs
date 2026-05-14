using System;
using MediatR;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.MealRecords.Commands.UpdateMealRecord;

/// <summary>
/// Öğün kaydı güncelleme isteği.
/// </summary>
public record UpdateMealRecordCommand(Guid MealRecordId, MealStatusType StatusType, string? Description) : IRequest;
