using System;
using System.Collections.Generic;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Okul entity'si.
/// </summary>
public class School : BaseEntity
{
    public string Name { get; private set; }
    
    private readonly List<Classroom> _classrooms = new();
    public IReadOnlyCollection<Classroom> Classrooms => _classrooms.AsReadOnly();

    public School(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Okulun ismini günceller.
    /// </summary>
    public void UpdateName(string newName)
    {
        Name = newName;
        UpdateTimestamp();
    }

    /// <summary>
    /// Okula yeni bir sınıf ekler.
    /// </summary>
    public void AddClassroom(Classroom classroom)
    {
        _classrooms.Add(classroom);
        UpdateTimestamp();
    }
}
