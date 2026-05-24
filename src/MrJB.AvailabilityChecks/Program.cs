using MrJB.AvailabilityChecks;
using MrJB.AvailabilityChecks.Domain.Configuration;

var builder = WebApplication.CreateBuilder(args);

// default logger

// configuration
var connectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

var applicationConfiguration = new ApplicationConfiguration();
builder.Configuration.GetSection(ApplicationConfiguration.Position).Bind(applicationConfiguration);
builder.Services.AddSingleton(applicationConfiguration);

var availaibilityConfigurations = builder.Configuration.GetSection("AvailabilityChecks").Get<List<AvailaibilityConfiguration>>() ?? [];
builder.Services.AddSingleton(availaibilityConfigurations);

// serilog

// http clients
builder.Services.AddHttpClient("availability")
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
    });

// application insights
builder.Services.AddApplicationInsightsTelemetryWorkerService(options => {
    options.ConnectionString = connectionString;
});

// bootstrapping
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// worker
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

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
