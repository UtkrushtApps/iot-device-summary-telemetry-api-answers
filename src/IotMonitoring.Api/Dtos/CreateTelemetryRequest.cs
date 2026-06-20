namespace IotMonitoring.Api.Dtos;

public class CreateTelemetryRequest
{
    public string? SensorId { get; set; }
    public double Temperature { get; set; }
    public int BatteryPercentage { get; set; }
    public DateTime RecordedAt { get; set; }
}

