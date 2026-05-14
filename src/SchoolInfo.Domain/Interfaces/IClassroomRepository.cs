using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Domain.Interfaces;

public interface IClassroomRepository
{
    Task<Classroom?> GetByIdAsync(Guid id, Guid schoolId);
    Task<List<Classroom>> GetAllBySchoolAsync(Guid schoolId);
    Task<List<Classroom>> GetByTeacherIdAsync(Guid teacherId, Guid schoolId);
    Task AddAsync(Classroom classroom);
    Task UpdateAsync(Classroom classroom);
}
