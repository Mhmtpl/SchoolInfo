using System;
using System.Collections.Generic;

namespace SchoolInfo.Application.Common.Exceptions;

/// <summary>
/// Validasyon hatalarÄ±nda fÄ±rlatÄ±lan istisna sÄ±nÄ±fÄ±.
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("Bir veya daha fazla doÄŸrulama hatasÄ± oluÅŸtu.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures) : this()
    {
        foreach (var failure in failures)
        {
            if (!Errors.ContainsKey(failure.PropertyName))
            {
                Errors[failure.PropertyName] = new[] { failure.ErrorMessage };
            }
        }
    }
}
