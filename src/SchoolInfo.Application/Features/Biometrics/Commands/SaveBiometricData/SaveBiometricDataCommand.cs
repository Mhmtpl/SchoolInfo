using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Biometrics.Commands.SaveBiometricData;

public record SaveBiometricDataCommand(
    string MacAddress,
    int? HeartRate,
    double? SpO2,
    double? BodyTemperature
) : IRequest<bool>;

public class SaveBiometricDataCommandHandler : IRequestHandler<SaveBiometricDataCommand, bool>
{
    private readonly IAppDbContext _dbContext;
    private readonly IBiometricBackgroundQueue _backgroundQueue;

    public SaveBiometricDataCommandHandler(IAppDbContext dbContext, IBiometricBackgroundQueue backgroundQueue)
    {
        _dbContext = dbContext;
        _backgroundQueue = backgroundQueue;
    }

    public async Task<bool> Handle(SaveBiometricDataCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MacAddress))
        {
            return false;
        }

        // MAC adresine sahip öğrenciyi bul (büyük/küçük harf duyarsız eşleştirme için Normalize edebiliriz)
        var targetMac = request.MacAddress.Trim().Replace("-", ":").ToUpperInvariant();

        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.SmartBandMacAddress != null && 
                                      s.SmartBandMacAddress.Replace("-", ":").ToUpper() == targetMac && 
                                      !s.IsDeleted, cancellationToken);

        if (student == null)
        {
            return false;
        }

        // Biyometrik kaydı oluştur
        var biometricRecord = new StudentBiometricRecord(
            student.Id,
            request.HeartRate,
            request.SpO2,
            request.BodyTemperature,
            DateTime.UtcNow,
            student.SchoolId
        );

        // Kuyruğa ekle
        await _backgroundQueue.QueueBiometricRecordAsync(biometricRecord);
        return true;
    }
}
