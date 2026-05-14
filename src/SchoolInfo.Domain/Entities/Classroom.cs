using System;
using System.Collections.Generic;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Sınıf entity'si. Bir sınıfta birden fazla öğretmen olabilir (anaokulu vb.).
/// </summary>
public class Classroom : BaseEntity
{
    public string Name { get; private set; }
    public Guid SchoolId { get; private set; }

    private readonly List<Student> _students = new();
    public IReadOnlyCollection<Student> Students => _students.AsReadOnly();

    private readonly List<User> _teachers = new();

    /// <summary>
    /// Sınıfa atanmış öğretmenler (birden fazla olabilir).
    /// </summary>
    public IReadOnlyCollection<User> Teachers => _teachers.AsReadOnly();

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

    /// <summary>
    /// Sınıfa öğretmen atar. Aynı öğretmen zaten atanmışsa tekrar eklemez.
    /// </summary>
    public void AddTeacher(User teacher)
    {
        if (_teachers.Exists(t => t.Id == teacher.Id)) return;
        _teachers.Add(teacher);
        UpdateTimestamp();
    }

    /// <summary>
    /// Sınıftan öğretmeni kaldırır.
    /// </summary>
    public void RemoveTeacher(User teacher)
    {
        var existing = _teachers.Find(t => t.Id == teacher.Id);
        if (existing != null)
        {
            _teachers.Remove(existing);
            UpdateTimestamp();
        }
    }
}
