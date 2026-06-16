namespace CoreMS.AvailabilityChecks.Domain.Configuration;

public class AvailaibilityConfiguration
{
    public const string Position = "AvailabilityChecks";

    public required string Name { get; set; }

    public required string Url { get; set; }

    public string? Environment { get; set; }

    public string? Location { get; set; }
}
