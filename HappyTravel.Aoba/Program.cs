using HappyTravel.Aoba.Infrastructure;
using HappyTravel.Infrastructure.Extensions;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureInfrastructure(options =>
{
    options.ConsulKey = Constants.ConsulKey;
});
builder.ConfigureServices();

var app = builder.Build();

app.UseRouting()
    .UseHttpMetrics()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapMetrics().RequireHost($"*:{builder.Configuration.GetValue<int>("HTDC_METRICS_PORT")}");
        endpoints.MapHealthChecks("/health").RequireHost($"*:{builder.Configuration.GetValue<int>("HTDC_HEALTH_PORT")}");
    });

app.Run();