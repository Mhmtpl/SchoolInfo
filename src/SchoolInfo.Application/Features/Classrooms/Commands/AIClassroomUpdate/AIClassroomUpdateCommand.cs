using System;
using System.Collections.Generic;
using MediatR;

namespace SchoolInfo.Application.Features.Classrooms.Commands.AIClassroomUpdate;

/// <summary>
/// Yapay zeka ile sınıf günlük ve yemek verilerini toplu güncelleme isteği.
/// </summary>
public record AIClassroomUpdateCommand(Guid ClassroomId, string Command, string DateStr) : IRequest<AIClassroomUpdateResultDto>;

/// <summary>
/// Yapay zeka güncelleme işlem sonucu.
/// </summary>
public record AIClassroomUpdateResultDto(bool Success, string Message, List<string> UpdatedStudents);
