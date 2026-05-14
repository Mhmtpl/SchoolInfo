using System;

namespace SchoolInfo.Domain.Exceptions;

/// <summary>
/// Ä°stenen Ã¶ÄŸrenci bulunamadÄ±ÄŸÄ±nda fÄ±rlatÄ±lan istisna.
/// </summary>
public class StudentNotFoundException : DomainException
{
    public StudentNotFoundException(Guid studentId) 
        : base($"Id'si {studentId} olan Ã¶ÄŸrenci bulunamadÄ±.")
    {
    }
}
