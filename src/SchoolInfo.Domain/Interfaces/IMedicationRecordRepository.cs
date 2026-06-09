using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Domain.Interfaces;

public interface IMedicationRecordRepository
{
    Task<MedicationRecord?> GetByIdAsync(Guid id);
    Task AddAsync(MedicationRecord entity);
    Task UpdateAsync(MedicationRecord entity);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<MedicationRecord>> GetByStudentAndDateAsync(Guid studentId, DateTime date);
}
