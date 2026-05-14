using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Infrastructure.Persistence.Repositories;

/// <summary>
/// Ã–ÄŸrenci verilerine eriÅŸimi saÄŸlayan repository sÄ±nÄ±fÄ±.
/// </summary>
public class StudentRepository : BaseRepository<Student>, IStudentRepository
{
    public StudentRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<Student>> GetByClassroomIdAsync(Guid classroomId)
    {
        return await DbContext.Students
            .Where(s => s.ClassroomId == classroomId)
            .ToListAsync();
    }
}
