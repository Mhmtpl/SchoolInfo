using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Domain.Interfaces;

/// <summary>
/// Aktivitelere eriÅŸim iÃ§in repository arayÃ¼zÃ¼.
/// </summary>
public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(Guid id);
    Task<IEnumerable<Activity>> GetByClassroomIdAsync(Guid classroomId, DateTime date);
    Task AddAsync(Activity activity);
    Task UpdateAsync(Activity activity);
    Task DeleteAsync(Guid id);
}
