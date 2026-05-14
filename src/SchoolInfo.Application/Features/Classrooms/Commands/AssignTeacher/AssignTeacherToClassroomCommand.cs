using System;
using MediatR;

namespace SchoolInfo.Application.Features.Classrooms.Commands.AssignTeacher;

/// <summary>
/// Bir sınıfa öğretmen atar. Aynı sınıfa birden fazla öğretmen atanabilir.
/// Sadece Admin yapabilir.
/// </summary>
public record AssignTeacherToClassroomCommand(Guid ClassroomId, Guid TeacherId) : IRequest;
