using IotMonitoring.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace IotMonitoring.Api.Data;

public class MonitoringDbContext : DbContext
{
    public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<TelemetryReading> TelemetryReadings => Set<TelemetryReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(200);
            entity.Property(d => d.Location).HasMaxLength(200);
        });

        modelBuilder.Entity<TelemetryReading>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.SensorId).HasMaxLength(100);
            entity.HasOne(r => r.Device)
                  .WithMany(d => d.Readings)
                  .HasForeignKey(r => r.DeviceId);
        });
    }
}

