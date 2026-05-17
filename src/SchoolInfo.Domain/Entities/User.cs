using System;
using System.Collections.Generic;
using SchoolInfo.Domain.Common;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// KullanÄ±cÄ± entity'si (Ã–ÄŸretmen, Veli veya YÃ¶netici).
/// </summary>
public class User : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; private set; }
    public string FcmToken { get; private set; }

    private readonly List<Student> _students = new();
    public IReadOnlyCollection<Student> Students => _students.AsReadOnly();

    private readonly List<Classroom> _classrooms = new();

    /// <summary>
    /// Öğretmenin atandığı sınıflar (birden fazla olabilir).
    /// </summary>
    public IReadOnlyCollection<Classroom> Classrooms => _classrooms.AsReadOnly();

    public User(string firstName, string lastName, string email, UserRole role)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Role = role;
        FcmToken = string.Empty;
    }

    /// <summary>
    /// KullanÄ±cÄ± bilgilerini gÃ¼nceller.
    /// </summary>
    public void UpdateDetails(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UpdateTimestamp();
    }

    /// <summary>
    /// KullanÄ±cÄ±nÄ±n rolÃ¼nÃ¼ deÄŸiÅŸtirir.
    /// </summary>
    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        UpdateTimestamp();
    }

    /// <summary>
    /// FCM Token (Bildirimler iÃ§in) gÃ¼nceller.
    /// </summary>
    public void UpdateFcmToken(string fcmToken)
    {
        FcmToken = fcmToken;
        UpdateTimestamp();
    }
}
