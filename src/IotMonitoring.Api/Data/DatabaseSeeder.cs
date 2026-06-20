using IotMonitoring.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace IotMonitoring.Api.Data;

public static class DatabaseSeeder
{
    public static async Task InitializeAsync(MonitoringDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Devices.AnyAsync())
        {
            return;
        }

        var baseTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var devices = new List<Device>
        {
            new Device { Id = 1, Name = "Boiler Sensor A", Location = "Plant 1" },
            new Device { Id = 2, Name = "Cold Storage Probe", Location = "Warehouse 3" },
            new Device { Id = 3, Name = "HVAC Controller", Location = "Building C" },
            new Device { Id = 4, Name = "Pipeline Flow Meter", Location = "Field 7" },
            new Device { Id = 5, Name = "Solar Inverter Node", Location = "Rooftop" }
        };

        await db.Devices.AddRangeAsync(devices);

        var readings = new List<TelemetryReading>();
        var nextId = 1;
        foreach (var device in devices)
        {
            for (var i = 0; i < 6; i++)
            {
                readings.Add(new TelemetryReading
                {
                    Id = nextId++,
                    DeviceId = device.Id,
                    SensorId = $"sensor-{device.Id}-{i % 2}",
                    Temperature = 20 + i + device.Id,
                    BatteryPercentage = 100 - (i * 5),
                    RecordedAt = baseTime.AddMinutes(i * 10 + device.Id)
                });
            }
        }

        await db.TelemetryReadings.AddRangeAsync(readings);
        await db.SaveChangesAsync();
    }
}

