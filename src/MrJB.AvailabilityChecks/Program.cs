using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.HttpOverrides;
using MrJB.AvailabilityChecks;
using MrJB.AvailabilityChecks.Domain.Configuration;

var builder = WebApplication.CreateBuilder(args);

/******************************************/
/*            configuration               */
/******************************************/

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

// azure app config
builder.ConfigureAzureAppConfiguration();

// application configuration
var applicationConfiguration = new ApplicationConfiguration();
builder.Configuration.GetSection(ApplicationConfiguration.Position).Bind(applicationConfiguration);
builder.Services.AddSingleton(applicationConfiguration);

/******************************************/
/*             application                */
/******************************************/

// configuration
var applicationInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

var availaibilityConfigurations = builder.Configuration.GetSection("AvailabilityChecks").Get<List<AvailaibilityConfiguration>>() ?? [];
builder.Services.AddSingleton(availaibilityConfigurations);

// http clients
builder.Services.AddHttpClient("availability")
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
    });

// Application Insights classic SDK for TrackAvailability
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = applicationInsightsConnectionString;
});

//// OpenTelemetry -> Azure Monitor / App Insights
//builder.Services
//    .AddOpenTelemetry()
//    .UseAzureMonitor(options =>
//    {
//        options.ConnectionString = applicationInsightsConnectionString;
//    });

//// Classic Application Insights client for TrackAvailability
//builder.Services.AddSingleton(sp =>
//{
//    var configuration = TelemetryConfiguration.CreateDefault();
//    configuration.ConnectionString = applicationInsightsConnectionString;

//    return new TelemetryClient(configuration);
//});

// bootstrapping
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// worker
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

/**************************************/
/*           forward headers          */
/**************************************/

#region forward headers

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};

// If you don’t want to maintain proxy IPs in-cluster:
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

#endregion

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
