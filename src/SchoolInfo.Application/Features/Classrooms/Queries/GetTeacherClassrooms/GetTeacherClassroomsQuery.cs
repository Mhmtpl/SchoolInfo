using System;
using System.Collections.Generic;
using MediatR;

namespace SchoolInfo.Application.Features.Classrooms.Queries.GetTeacherClassrooms;

public record TeacherClassroomDto(Guid Id, string Name);

public record GetTeacherClassroomsQuery() : IRequest<List<TeacherClassroomDto>>;
