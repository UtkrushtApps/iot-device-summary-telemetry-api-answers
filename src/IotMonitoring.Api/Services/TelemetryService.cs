using IotMonitoring.Api.Data;
using IotMonitoring.Api.Dtos;
using IotMonitoring.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace IotMonitoring.Api.Services;

public enum TelemetryResultStatus
{
    Created,
    InvalidInput,
    DeviceNotFound
}

public class TelemetryResult
{
    public TelemetryResultStatus Status { get; set; }
    public string? Error { get; set; }
    public int? ReadingId { get; set; }
}

public interface ITelemetryService
{
    Task<TelemetryResult> AddReadingAsync(int deviceId, CreateTelemetryRequest request, CancellationToken cancellationToken);
}

public class TelemetryService : ITelemetryService
{
    private readonly MonitoringDbContext _db;

    public TelemetryService(MonitoringDbContext db)
    {
        _db = db;
    }

    public async Task<TelemetryResult> AddReadingAsync(int deviceId, CreateTelemetryRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            return new TelemetryResult
            {
                Status = TelemetryResultStatus.InvalidInput,
                Error = "Telemetry request body is required."
            };
        }

        if (string.IsNullOrWhiteSpace(request.SensorId))
        {
            return new TelemetryResult
            {
                Status = TelemetryResultStatus.InvalidInput,
                Error = "SensorId is required."
            };
        }

        if (request.BatteryPercentage < 0 || request.BatteryPercentage > 100)
        {
            return new TelemetryResult
            {
                Status = TelemetryResultStatus.InvalidInput,
                Error = "BatteryPercentage must be between 0 and 100."
            };
        }

        var deviceExists = await _db.Devices
            .AsNoTracking()
            .AnyAsync(d => d.Id == deviceId, cancellationToken);

        if (!deviceExists)
        {
            return new TelemetryResult
            {
                Status = TelemetryResultStatus.DeviceNotFound,
                Error = $"Device {deviceId} was not found."
            };
        }

        var reading = new TelemetryReading
        {
            DeviceId = deviceId,
            SensorId = request.SensorId,
            Temperature = request.Temperature,
            BatteryPercentage = request.BatteryPercentage,
            RecordedAt = request.RecordedAt
        };

        _db.TelemetryReadings.Add(reading);
        await _db.SaveChangesAsync(cancellationToken);

        return new TelemetryResult
        {
            Status = TelemetryResultStatus.Created,
            ReadingId = reading.Id
        };
    }
}

