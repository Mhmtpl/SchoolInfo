using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Domain.Interfaces;

public interface IMedicationRecordRepository
{
    Task<IEnumerable<MedicationRecord>> GetByStudentAndDateAsync(Guid studentId, DateTime date);
}
