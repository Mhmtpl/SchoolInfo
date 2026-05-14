using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Öğrenci entity'si.
/// </summary>
public class Student : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public Guid ClassroomId { get; private set; }

    public Student(string firstName, string lastName, DateTime dateOfBirth, Guid classroomId)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        ClassroomId = classroomId;
    }

    /// <summary>
    /// Öğrenci bilgilerini günceller.
    /// </summary>
    public void UpdateInfo(string firstName, string lastName, DateTime dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        UpdateTimestamp();
    }

    /// <summary>
    /// Öğrencinin sınıfını değiştirir.
    /// </summary>
    public void ChangeClassroom(Guid newClassroomId)
    {
        ClassroomId = newClassroomId;
        UpdateTimestamp();
    }
}
