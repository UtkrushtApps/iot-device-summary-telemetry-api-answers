namespace IotMonitoring.Api.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public ICollection<TelemetryReading> Readings { get; set; } = new List<TelemetryReading>();
}

