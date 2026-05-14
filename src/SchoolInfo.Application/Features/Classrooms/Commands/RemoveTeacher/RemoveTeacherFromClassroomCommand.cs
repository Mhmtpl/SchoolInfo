using System;
using MediatR;

namespace SchoolInfo.Application.Features.Classrooms.Commands.RemoveTeacher;

/// <summary>
/// Bir sınıftan öğretmeni kaldırır. Sadece Admin yapabilir.
/// </summary>
public record RemoveTeacherFromClassroomCommand(Guid ClassroomId, Guid TeacherId) : IRequest;
