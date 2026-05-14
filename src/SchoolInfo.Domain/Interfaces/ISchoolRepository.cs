using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Domain.Interfaces;

public interface ISchoolRepository
{
    Task<School?> GetByIdAsync(Guid id);
    Task<List<School>> GetAllAsync();
    Task AddAsync(School school);
}
