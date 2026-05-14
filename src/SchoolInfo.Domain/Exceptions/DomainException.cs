using System;

namespace SchoolInfo.Domain.Exceptions;

/// <summary>
/// Domain kurallarÄ± ihlal edildiÄŸinde fÄ±rlatÄ±lan temel istisna sÄ±nÄ±fÄ±.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
