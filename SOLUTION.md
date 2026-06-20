# Solution Steps

1. Replace the per-device loop in DeviceSummaryService.GetSummaryAsync with one set-based EF Core query over Devices.

2. Project directly to DeviceSummaryDto inside the query, using the navigation collection to compute ReadingCount and the latest telemetry fields with OrderByDescending(...).Select(...).FirstOrDefault().

3. Call ToListAsync(cancellationToken) only once so the endpoint performs a constant number of async database materializations as the device fleet grows.

4. Use AsNoTracking for read-only summary and existence-check queries, and pass the CancellationToken to all async EF Core operations.

5. In TelemetryService.AddReadingAsync, check cancellation at the start, then validate that the request exists, SensorId is not null/blank, and BatteryPercentage is between 0 and 100 before adding anything to the DbContext.

6. After field validation, verify the target device exists with AnyAsync(d => d.Id == deviceId, cancellationToken). Return DeviceNotFound if it does not exist.

7. Only construct and add a TelemetryReading after all validation and device existence checks have passed, then persist it with SaveChangesAsync(cancellationToken).

8. Update DevicesController.AddTelemetry to map service result statuses to HTTP responses: success remains 200 OK, invalid input returns 400 Bad Request, and unknown device returns 404 Not Found.

9. Build the solution and run the tests with dotnet test, or use the provided run.sh script when SQL Server is available.

