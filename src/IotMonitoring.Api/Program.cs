using IotMonitoring.Api.Data;
using IotMonitoring.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string ConnectionString =
    "Server=127.0.0.1,1433;Database=IotMonitoring;User Id=sa;Password=Your_strong_Pass123;TrustServerCertificate=True;";

builder.Services.AddDbContext<MonitoringDbContext>(options =>
    options.UseSqlServer(ConnectionString));

builder.Services.AddScoped<IDeviceSummaryService, DeviceSummaryService>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MonitoringDbContext>();
    await DatabaseSeeder.InitializeAsync(db);
}

app.MapControllers();

app.Run();

public partial class Program { }

