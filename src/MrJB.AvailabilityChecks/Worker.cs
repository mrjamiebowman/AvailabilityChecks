using Microsoft.ApplicationInsights;
using MrJB.AvailabilityChecks.Domain.Configuration;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MrJB.AvailabilityChecks;

public class Worker : BackgroundService
{
    // logging
    private readonly ILogger<Worker> _logger;

    // services
    private readonly TelemetryClient _telemetryClient;

    //private static readonly Meter Meter = new("InternalAvailability");

    //private static readonly Counter<long> AvailabilityFailures = Meter.CreateCounter<long>("availability_check_failures");

    //private static readonly Histogram<double> AvailabilityDurationMs = Meter.CreateHistogram<double>("availability_check_duration_ms");

    private readonly IHttpClientFactory _httpClientFactory;

    // configuration
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly List<AvailaibilityConfiguration> _availaibilityConfiguration;

    public Worker(
        ILogger<Worker> logger,
        IHttpClientFactory httpClientFactory,
        TelemetryClient telemetryClient,
        ApplicationConfiguration applicationConfiguration,
        List<AvailaibilityConfiguration> availaibilityConfiguration
        )
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _telemetryClient = telemetryClient;
        _applicationConfiguration = applicationConfiguration;
        _availaibilityConfiguration = availaibilityConfiguration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //foreach (var site in sites)
            //{
            //    await CheckSiteAsync(site, stoppingToken);
            //}

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    //private async Task CheckSiteAsync(
    //    AvailabilityCheckOptions site,
    //    CancellationToken cancellationToken)
    //{
    //    var client = _httpClientFactory.CreateClient("availability");

    //    var stopwatch = Stopwatch.StartNew();
    //    var timestamp = DateTimeOffset.UtcNow;

    //    var success = false;
    //    string? message = null;
    //    int? statusCode = null;

    //    try
    //    {
    //        using var request = new HttpRequestMessage(HttpMethod.Get, site.Url);

    //        using var response = await client.SendAsync(request, cancellationToken);

    //        statusCode = (int)response.StatusCode;
    //        success = response.IsSuccessStatusCode;

    //        if (!success)
    //        {
    //            message = $"Unexpected status code: {(int)response.StatusCode} {response.ReasonPhrase}";
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        success = false;
    //        message = ex.Message;

    //        _logger.LogError(
    //            ex,
    //            "Availability check failed for {Name} at {Url}",
    //            site.Name,
    //            site.Url);
    //    }
    //    finally
    //    {
    //        stopwatch.Stop();
    //    }

    //    var duration = stopwatch.Elapsed;

    //    var properties = new Dictionary<string, string>
    //    {
    //        ["url"] = site.Url,
    //        ["environment"] = site.Environment ?? "unknown"
    //    };

    //    if (statusCode.HasValue)
    //    {
    //        properties["statusCode"] = statusCode.Value.ToString();
    //    }

    //    var metrics = new Dictionary<string, double>
    //    {
    //        ["durationMs"] = duration.TotalMilliseconds
    //    };

    //    // This is what makes it show up in the Application Insights Availability blade.
    //    _telemetryClient.TrackAvailability(
    //        name: site.Name,
    //        timeStamp: timestamp,
    //        duration: duration,
    //        runLocation: site.Location ?? Environment.MachineName,
    //        success: success,
    //        message: message,
    //        properties: properties,
    //        metrics: metrics);

    //    // Optional but useful for OTel / Azure Monitor Metrics / alerts.
    //    AvailabilityDurationMs.Record(
    //        duration.TotalMilliseconds,
    //        new KeyValuePair<string, object?>("site", site.Name),
    //        new KeyValuePair<string, object?>("environment", site.Environment),
    //        new KeyValuePair<string, object?>("success", success));

    //    if (!success)
    //    {
    //        AvailabilityFailures.Add(
    //            1,
    //            new KeyValuePair<string, object?>("site", site.Name),
    //            new KeyValuePair<string, object?>("environment", site.Environment));
    //    }

    //    _logger.LogInformation(
    //        "Availability check {Name} {Success} in {DurationMs}ms",
    //        site.Name,
    //        success ? "succeeded" : "failed",
    //        duration.TotalMilliseconds);

    //    _telemetryClient.Flush();
    //}
}

