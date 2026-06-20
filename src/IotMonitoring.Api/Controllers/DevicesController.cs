using IotMonitoring.Api.Dtos;
using IotMonitoring.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IotMonitoring.Api.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceSummaryService _summaryService;
    private readonly ITelemetryService _telemetryService;

    public DevicesController(IDeviceSummaryService summaryService, ITelemetryService telemetryService)
    {
        _summaryService = summaryService;
        _telemetryService = telemetryService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<List<DeviceSummaryDto>>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _summaryService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    [HttpPost("{deviceId:int}/telemetry")]
    public async Task<ActionResult> AddTelemetry(int deviceId, [FromBody] CreateTelemetryRequest request, CancellationToken cancellationToken)
    {
        var result = await _telemetryService.AddReadingAsync(deviceId, request, cancellationToken);

        return result.Status switch
        {
            TelemetryResultStatus.Created => Ok(new { result.ReadingId, status = result.Status.ToString() }),
            TelemetryResultStatus.InvalidInput => BadRequest(new { error = result.Error, status = result.Status.ToString() }),
            TelemetryResultStatus.DeviceNotFound => NotFound(new { error = result.Error, status = result.Status.ToString() }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { error = "Unexpected telemetry result.", status = result.Status.ToString() })
        };
    }
}

