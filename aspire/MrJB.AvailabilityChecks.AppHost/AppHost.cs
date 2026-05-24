var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MrJB_AvailabilityChecks>("mrjb-availabilitychecks");

builder.Build().Run();
