using System;

namespace SchoolInfo.Domain.Exceptions;

/// <summary>
/// Domain kuralları ihlal edildiğinde fırlatılan temel istisna sınıfı.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
