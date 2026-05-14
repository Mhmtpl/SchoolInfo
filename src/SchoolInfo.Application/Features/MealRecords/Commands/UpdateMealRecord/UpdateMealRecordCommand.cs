using System;
using MediatR;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.MealRecords.Commands.UpdateMealRecord;

/// <summary>
/// Ã–ÄŸÃ¼n kaydÄ± gÃ¼ncelleme isteÄŸi.
/// </summary>
public record UpdateMealRecordCommand(Guid MealRecordId, MealStatusType StatusType, string? Description) : IRequest;
