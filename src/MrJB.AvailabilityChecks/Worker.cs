using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using MrJB.AvailabilityChecks.Domain.Configuration;
using MrJB.AvailabilityChecks.ServiceDefaults;
using System.Diagnostics;

namespace MrJB.AvailabilityChecks;

public class Worker : BackgroundService
{
    // logging
    private readonly ILogger<Worker> _logger;

    // services
    private readonly TelemetryClient _telemetryClient;

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
        _logger.LogInformation("Starting Availability Checks Worker...");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = OTel.ActivitySource.StartActivity($"{nameof(Worker)}.{nameof(ExecuteAsync)}");

            // task list
            List<Task> processSitesTask = new List<Task>();

            foreach (var site in _availaibilityConfiguration)
            {
                processSitesTask.Add(CheckSiteAsync(site, stoppingToken));
            }

            // process
            await Task.WhenAll(processSitesTask);

            activity?.Stop();

            // wait...
            await Task.Delay(TimeSpan.FromMinutes(_applicationConfiguration.Delay), stoppingToken);
        }
    }

    private async Task CheckSiteAsync(AvailaibilityConfiguration site, CancellationToken cancellationToken)
    {
        using var activity = OTel.ActivitySource.StartActivity($"{nameof(Worker)}.{nameof(CheckSiteAsync)}");
        activity?.AddTag(Spans.Site, site);

        var client = _httpClientFactory.CreateClient("availability");

        var stopwatch = Stopwatch.StartNew();
        var timestamp = DateTimeOffset.UtcNow;

        var success = false;
        string? message = null;
        int? statusCode = null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, site.Url);
            using var response = await client.SendAsync(request, cancellationToken);

            statusCode = (int)response.StatusCode;
            success = response.IsSuccessStatusCode;

            if (!success)
            {
                message = $"Unexpected status code: {(int)response.StatusCode} {response.ReasonPhrase}";
                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.AddTag(Spans.Status, Spans.Values.Failure);
            }
            else
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
                activity?.AddTag(Spans.Status, Spans.Values.Success);
            }
        }
        catch (Exception ex)
        {
            success = false;
            message = ex.Message;

            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogError(
                ex,
                "Availability check failed for {name} at {url}",
                site.Name,
                site.Url);
        }
        finally
        {
            stopwatch.Stop();
        }

        var duration = stopwatch.Elapsed;

        if (success == true)
        {
            _logger.LogInformation(
                "Availability check for {name} at {url}, ms {milliseconds}",
                site.Name,
                site.Url,
                duration
            );
        }

        var properties = new Dictionary<string, string>
        {
            [Spans.Url] = site.Url,
            [Spans.Environment] = site.Environment ?? "unknown"
        };

        if (statusCode.HasValue)
        {
            properties[Spans.StatusCode] = statusCode.Value.ToString();
        }

        var metrics = new Dictionary<string, double>
        {
            [Spans.DurationMs] = duration.TotalMilliseconds
        };

        // This is what makes it show up in the Application Insights Availability blade.
        var availability = new AvailabilityTelemetry
        {
            Name = site.Name,
            RunLocation = site.Location ?? Environment.MachineName,
            Timestamp = timestamp,
            Duration = duration,
            Success = success,
            Message = message
        };

        availability.Properties["url"] = site.Url;
        availability.Properties["machine"] = Environment.MachineName;
        availability.Properties["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";

        _telemetryClient.TrackAvailability(availability);

        // Optional but useful for OTel / Azure Monitor Metrics / alerts.
        OTel.Meters.AvailabilityDurationMs.Record(duration.TotalMilliseconds,
            new KeyValuePair<string, object?>(Spans.Site, site.Name),
            new KeyValuePair<string, object?>(Spans.Environment, site.Environment),
            new KeyValuePair<string, object?>(Spans.Status, success)
        );

        var tagList = new TagList() {
            new KeyValuePair<string, object?>(Spans.Site, site.Name),
            new KeyValuePair<string, object?>(Spans.Environment, site.Environment),
            new KeyValuePair<string, object?>(Spans.Status, success)
        };

        OTel.Meters.AddAvailabilityCheck(1, tagList);

        _logger.LogInformation("Availability check {Name} {Success} in {DurationMs}ms",
            site.Name,
            success ? "succeeded" : "failed",
            duration.TotalMilliseconds);

        await _telemetryClient.FlushAsync(cancellationToken);
    }
}
