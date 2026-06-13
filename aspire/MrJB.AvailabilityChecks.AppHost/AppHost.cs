var builder = DistributedApplication.CreateBuilder(args);

// launch profiles
var launchProfile = builder.Configuration["LaunchProfile"] ?? "PROD";

// projects
builder.AddProject<Projects.MrJB_AvailabilityChecks>("availabilitychecks", launchProfile);

builder.Build().Run();
