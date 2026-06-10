using System;
using MediatR;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.MealRecords.Commands.UpdateMealRecord;

/// <summary>
/// Ã–ÄŸÃ¼n kaydÄ± gÃ¼ncelleme isteÄŸi.
/// </summary>
public record UpdateMealRecordCommand(Guid MealRecordId, Guid? StudentId, string? MealName, MealStatusType StatusType, string? Description) : IRequest;
