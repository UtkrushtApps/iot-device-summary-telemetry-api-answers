namespace IotMonitoring.Api.Dtos;

public class DeviceSummaryDto
{
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int ReadingCount { get; set; }
    public double? LatestTemperature { get; set; }
    public int? LatestBatteryPercentage { get; set; }
    public DateTime? LatestRecordedAt { get; set; }
}

