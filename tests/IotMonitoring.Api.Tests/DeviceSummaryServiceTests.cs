using IotMonitoring.Api.Data;
using IotMonitoring.Api.Models;
using IotMonitoring.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IotMonitoring.Api.Tests;

public class DeviceSummaryServiceTests
{
    private static MonitoringDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MonitoringDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new MonitoringDbContext(options);

        var baseTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        db.Devices.Add(new Device { Id = 1, Name = "Boiler Sensor A", Location = "Plant 1" });
        db.Devices.Add(new Device { Id = 2, Name = "Cold Storage Probe", Location = "Warehouse 3" });
        db.Devices.Add(new Device { Id = 3, Name = "Empty Device", Location = "Nowhere" });

        db.TelemetryReadings.Add(new TelemetryReading { Id = 1, DeviceId = 1, SensorId = "s-1", Temperature = 20, BatteryPercentage = 90, RecordedAt = baseTime.AddMinutes(10) });
        db.TelemetryReadings.Add(new TelemetryReading { Id = 2, DeviceId = 1, SensorId = "s-1", Temperature = 22, BatteryPercentage = 85, RecordedAt = baseTime.AddMinutes(30) });
        db.TelemetryReadings.Add(new TelemetryReading { Id = 3, DeviceId = 2, SensorId = "s-2", Temperature = 5, BatteryPercentage = 60, RecordedAt = baseTime.AddMinutes(5) });

        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task Summary_ReturnsOneEntryPerDevice()
    {
        using var db = CreateContext();
        var service = new DeviceSummaryService(db);

        var summary = await service.GetSummaryAsync(CancellationToken.None);

        Assert.Equal(3, summary.Count);
    }

    [Fact]
    public async Task Summary_ReportsCorrectLatestReadingAndCount()
    {
        using var db = CreateContext();
        var service = new DeviceSummaryService(db);

        var summary = await service.GetSummaryAsync(CancellationToken.None);

        var device1 = summary.Single(s => s.DeviceId == 1);
        Assert.Equal(2, device1.ReadingCount);
        Assert.Equal(22, device1.LatestTemperature);
        Assert.Equal(85, device1.LatestBatteryPercentage);

        var device3 = summary.Single(s => s.DeviceId == 3);
        Assert.Equal(0, device3.ReadingCount);
        Assert.Null(device3.LatestTemperature);
        Assert.Null(device3.LatestRecordedAt);
    }

    [Fact]
    public async Task Summary_DoesNotIssueQueryPerDevice()
    {
        using var db = CreateContext();
        var counter = new QueryCountInterceptorState();
        QueryCountInterceptorState.Current = counter;

        var service = new DeviceSummaryService(db);
        await service.GetSummaryAsync(CancellationToken.None);

        QueryCountInterceptorState.Current = null;

        Assert.True(
            counter.MaterializationCount <= 2,
            $"Expected the summary to be produced without a query per device, but observed {counter.MaterializationCount} query materializations.");
    }
}

public class QueryCountInterceptorState
{
    public static QueryCountInterceptorState? Current;
    public int MaterializationCount;
}

