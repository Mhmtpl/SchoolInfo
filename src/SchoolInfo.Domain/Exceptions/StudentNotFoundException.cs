using System;

namespace SchoolInfo.Domain.Exceptions;

/// <summary>
/// İstenen öğrenci bulunamadığında fırlatılan istisna.
/// </summary>
public class StudentNotFoundException : DomainException
{
    public StudentNotFoundException(Guid studentId) 
        : base($"Id'si {studentId} olan öğrenci bulunamadı.")
    {
    }
}
