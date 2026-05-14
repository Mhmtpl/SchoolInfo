using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Domain.Interfaces;

/// <summary>
/// Ã–ÄŸrenci verilerine eriÅŸim iÃ§in repository arayÃ¼zÃ¼.
/// </summary>
public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id);
    Task<IEnumerable<Student>> GetByClassroomIdAsync(Guid classroomId);
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(Guid id);
}
