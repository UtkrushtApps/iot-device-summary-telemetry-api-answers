using IotMonitoring.Api.Data;
using IotMonitoring.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace IotMonitoring.Api.Services;

public interface IDeviceSummaryService
{
    Task<List<DeviceSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken);
}

public class DeviceSummaryService : IDeviceSummaryService
{
    private readonly MonitoringDbContext _db;

    public DeviceSummaryService(MonitoringDbContext db)
    {
        _db = db;
    }

    public Task<List<DeviceSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return _db.Devices
            .AsNoTracking()
            .OrderBy(d => d.Id)
            .Select(d => new DeviceSummaryDto
            {
                DeviceId = d.Id,
                DeviceName = d.Name,
                Location = d.Location,
                ReadingCount = d.Readings.Count(),
                LatestTemperature = d.Readings
                    .OrderByDescending(r => r.RecordedAt)
                    .Select(r => (double?)r.Temperature)
                    .FirstOrDefault(),
                LatestBatteryPercentage = d.Readings
                    .OrderByDescending(r => r.RecordedAt)
                    .Select(r => (int?)r.BatteryPercentage)
                    .FirstOrDefault(),
                LatestRecordedAt = d.Readings
                    .OrderByDescending(r => r.RecordedAt)
                    .Select(r => (DateTime?)r.RecordedAt)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
    }
}

