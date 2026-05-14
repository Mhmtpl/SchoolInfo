using System;

namespace SchoolInfo.Domain.Exceptions;

/// <summary>
/// Yetkisiz bir sınıfa erişilmeye çalışıldığında fırlatılan istisna.
/// </summary>
public class UnauthorizedClassroomAccessException : DomainException
{
    public UnauthorizedClassroomAccessException(Guid userId, Guid classroomId) 
        : base($"Id'si {userId} olan kullanıcının {classroomId} id'li sınıfa erişim yetkisi yok.")
    {
    }
}
