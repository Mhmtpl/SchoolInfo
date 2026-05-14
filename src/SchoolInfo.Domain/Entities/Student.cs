using System;
using System.Collections.Generic;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Ã–ÄŸrenci entity'si.
/// </summary>
public class Student : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public Guid ClassroomId { get; private set; }

    private readonly List<User> _parents = new();
    public IReadOnlyCollection<User> Parents => _parents.AsReadOnly();

    public Student(string firstName, string lastName, DateTime dateOfBirth, Guid classroomId)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        ClassroomId = classroomId;
    }

    /// <summary>
    /// Ã–ÄŸrenci bilgilerini gÃ¼nceller.
    /// </summary>
    public void UpdateInfo(string firstName, string lastName, DateTime dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        UpdateTimestamp();
    }

    /// <summary>
    /// Ã–ÄŸrencinin sÄ±nÄ±fÄ±nÄ± deÄŸiÅŸtirir.
    /// </summary>
    public void ChangeClassroom(Guid newClassroomId)
    {
        ClassroomId = newClassroomId;
        UpdateTimestamp();
    }

    /// <summary>
    /// Ã–ÄŸrenciye veli ekler.
    /// </summary>
    public void AddParent(User parent)
    {
        if (!_parents.Contains(parent))
        {
            _parents.Add(parent);
            UpdateTimestamp();
        }
    }

    /// <summary>
    /// Ã–ÄŸrenciden veli baÄŸlantÄ±sÄ±nÄ± kaldÄ±rÄ±r.
    /// </summary>
    public void RemoveParent(User parent)
    {
        if (_parents.Contains(parent))
        {
            _parents.Remove(parent);
            UpdateTimestamp();
        }
    }
}
