using IotMonitoring.Api.Data;
using IotMonitoring.Api.Dtos;
using IotMonitoring.Api.Models;
using IotMonitoring.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IotMonitoring.Api.Tests;

public class TelemetryServiceTests
{
    private static MonitoringDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MonitoringDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new MonitoringDbContext(options);
        db.Devices.Add(new Device { Id = 1, Name = "Boiler Sensor A", Location = "Plant 1" });
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task ValidReading_IsPersisted()
    {
        using var db = CreateContext();
        var service = new TelemetryService(db);

        var request = new CreateTelemetryRequest
        {
            SensorId = "sensor-1-0",
            Temperature = 25.5,
            BatteryPercentage = 80,
            RecordedAt = DateTime.UtcNow
        };

        var result = await service.AddReadingAsync(1, request, CancellationToken.None);

        Assert.Equal(TelemetryResultStatus.Created, result.Status);
        Assert.Equal(1, await db.TelemetryReadings.CountAsync());
    }

    [Fact]
    public async Task BlankSensorId_IsRejected_AndNotPersisted()
    {
        using var db = CreateContext();
        var service = new TelemetryService(db);

        var request = new CreateTelemetryRequest
        {
            SensorId = "   ",
            Temperature = 25.5,
            BatteryPercentage = 80,
            RecordedAt = DateTime.UtcNow
        };

        var result = await service.AddReadingAsync(1, request, CancellationToken.None);

        Assert.Equal(TelemetryResultStatus.InvalidInput, result.Status);
        Assert.Equal(0, await db.TelemetryReadings.CountAsync());
    }

    [Fact]
    public async Task BatteryAbove100_IsRejected_AndNotPersisted()
    {
        using var db = CreateContext();
        var service = new TelemetryService(db);

        var request = new CreateTelemetryRequest
        {
            SensorId = "sensor-1-0",
            Temperature = 25.5,
            BatteryPercentage = 150,
            RecordedAt = DateTime.UtcNow
        };

        var result = await service.AddReadingAsync(1, request, CancellationToken.None);

        Assert.Equal(TelemetryResultStatus.InvalidInput, result.Status);
        Assert.Equal(0, await db.TelemetryReadings.CountAsync());
    }

    [Fact]
    public async Task NegativeBattery_IsRejected_AndNotPersisted()
    {
        using var db = CreateContext();
        var service = new TelemetryService(db);

        var request = new CreateTelemetryRequest
        {
            SensorId = "sensor-1-0",
            Temperature = 25.5,
            BatteryPercentage = -5,
            RecordedAt = DateTime.UtcNow
        };

        var result = await service.AddReadingAsync(1, request, CancellationToken.None);

        Assert.Equal(TelemetryResultStatus.InvalidInput, result.Status);
        Assert.Equal(0, await db.TelemetryReadings.CountAsync());
    }

    [Fact]
    public async Task UnknownDevice_IsRejected_AndNotPersisted()
    {
        using var db = CreateContext();
        var service = new TelemetryService(db);

        var request = new CreateTelemetryRequest
        {
            SensorId = "sensor-9-0",
            Temperature = 25.5,
            BatteryPercentage = 80,
            RecordedAt = DateTime.UtcNow
        };

        var result = await service.AddReadingAsync(999, request, CancellationToken.None);

        Assert.Equal(TelemetryResultStatus.DeviceNotFound, result.Status);
        Assert.Equal(0, await db.TelemetryReadings.CountAsync());
    }
}

