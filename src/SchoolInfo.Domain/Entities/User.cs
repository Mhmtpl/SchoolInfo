using System;
using SchoolInfo.Domain.Common;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Kullanıcı entity'si (Öğretmen, Veli veya Yönetici).
/// </summary>
public class User : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; private set; }

    public User(string firstName, string lastName, string email, UserRole role)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Role = role;
    }

    /// <summary>
    /// Kullanıcı bilgilerini günceller.
    /// </summary>
    public void UpdateDetails(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UpdateTimestamp();
    }

    /// <summary>
    /// Kullanıcının rolünü değiştirir.
    /// </summary>
    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        UpdateTimestamp();
    }
}
