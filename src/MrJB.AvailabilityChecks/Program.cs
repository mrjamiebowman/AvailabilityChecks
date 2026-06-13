using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.HttpOverrides;
using MrJB.AvailabilityChecks;
using MrJB.AvailabilityChecks.Domain.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

/******************************************/
/*                logging                 */
/******************************************/

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        theme: AnsiConsoleTheme.Literate,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}"
    )
    .CreateBootstrapLogger();

// start up log
Log.Logger.Information("Starting up AvailabilityChecks, Environment: {environment}", builder.Environment.EnvironmentName);

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
/*                serilog                 */
/******************************************/

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    var otlpEndpoint = context.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(
            theme: AnsiConsoleTheme.Literate,
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}"
        );

    // serilog dumps the default logger and replaces it...
    // without this serilog will block logs being shipped to otel/aspire.
    // i.e., removet his an structured logs goes away...
    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        loggerConfiguration.WriteTo.OpenTelemetry(options =>
        {
            options.Endpoint = otlpEndpoint;
        });
    }
});

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

// OpenTelemetry -> Azure Monitor / App Insights
builder.Services
    .AddOpenTelemetry()
    .UseAzureMonitor(options =>
    {
        options.ConnectionString = applicationInsightsConnectionString;
    });

// Classic Application Insights client for TrackAvailability
builder.Services.AddSingleton(sp =>
{
    var configuration = TelemetryConfiguration.CreateDefault();
    configuration.ConnectionString = applicationInsightsConnectionString;

    return new TelemetryClient(configuration);
});

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
