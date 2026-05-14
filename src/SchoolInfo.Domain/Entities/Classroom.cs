using System;
using System.Collections.Generic;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Sınıf entity'si.
/// </summary>
public class Classroom : BaseEntity
{
    public string Name { get; private set; }
    public Guid SchoolId { get; private set; }

    private readonly List<Student> _students = new();
    public IReadOnlyCollection<Student> Students => _students.AsReadOnly();

    public Classroom(string name, Guid schoolId)
    {
        Name = name;
        SchoolId = schoolId;
    }

    /// <summary>
    /// Sınıfın ismini günceller.
    /// </summary>
    public void UpdateName(string newName)
    {
        Name = newName;
        UpdateTimestamp();
    }

    /// <summary>
    /// Sınıfa öğrenci ekler.
    /// </summary>
    public void AddStudent(Student student)
    {
        _students.Add(student);
        UpdateTimestamp();
    }
}
