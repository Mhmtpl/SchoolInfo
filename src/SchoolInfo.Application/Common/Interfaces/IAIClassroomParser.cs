using System.Threading.Tasks;

namespace SchoolInfo.Application.Common.Interfaces;

/// <summary>
/// Öğretmenin sesli veya yazılı sınıf güncelleme komutlarını analiz eden servis.
/// </summary>
public interface IAIClassroomParser
{
    /// <summary>
    /// Öğretmen komutunu ve öğrencilerin listesini alarak güncellemeleri yapılandırılmış JSON olarak döner.
    /// </summary>
    Task<string> ParseClassroomCommandAsync(string command, string studentNamesJson);
}
