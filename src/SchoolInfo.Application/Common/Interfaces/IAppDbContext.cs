using System.Threading;
using System.Threading.Tasks;

namespace SchoolInfo.Application.Common.Interfaces;

/// <summary>
/// Veritabanı işlemlerini UnitOfWork kullanmadan gerçekleştirmek için AppDbContext arayüzü.
/// </summary>
public interface IAppDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
