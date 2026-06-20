namespace IotMonitoring.Api.Models;

public class TelemetryReading
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public string? SensorId { get; set; }
    public double Temperature { get; set; }
    public int BatteryPercentage { get; set; }
    public DateTime RecordedAt { get; set; }
    public Device? Device { get; set; }
}

