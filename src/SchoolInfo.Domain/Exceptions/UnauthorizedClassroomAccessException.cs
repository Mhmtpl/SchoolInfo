using System;

namespace SchoolInfo.Domain.Exceptions;

/// <summary>
/// Yetkisiz bir sÄ±nÄ±fa eriÅŸilmeye Ã§alÄ±ÅŸÄ±ldÄ±ÄŸÄ±nda fÄ±rlatÄ±lan istisna.
/// </summary>
public class UnauthorizedClassroomAccessException : DomainException
{
    public UnauthorizedClassroomAccessException(Guid userId, Guid classroomId) 
        : base($"Id'si {userId} olan kullanÄ±cÄ±nÄ±n {classroomId} id'li sÄ±nÄ±fa eriÅŸim yetkisi yok.")
    {
    }
}
