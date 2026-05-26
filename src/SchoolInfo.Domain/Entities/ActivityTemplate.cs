using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Entities;

public class ActivityTemplate : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public string RequiredMaterials { get; private set; }
    public string AgeGroup { get; private set; }

    public ActivityTemplate(string title, string description, string category, string requiredMaterials, string ageGroup)
    {
        Title = title;
        Description = description;
        Category = category;
        RequiredMaterials = requiredMaterials;
        AgeGroup = ageGroup;
    }

    public void Update(string title, string description, string category, string requiredMaterials, string ageGroup)
    {
        Title = title;
        Description = description;
        Category = category;
        RequiredMaterials = requiredMaterials;
        AgeGroup = ageGroup;
        UpdateTimestamp();
    }
}
