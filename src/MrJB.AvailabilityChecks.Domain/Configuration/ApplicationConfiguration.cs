namespace MrJB.AvailabilityChecks.Domain.Configuration;

public class ApplicationConfiguration
{
    public const string Position = "Application";

    /// <summary>
    ///  Typically, we check every 20 minutes.
    /// </summary>
    public int Delay { get; set; } = 20;

    /// <summary>
    ///  How many availability checks can run simultaneously?
    /// </summary>
    public int Throughput { get; set; } = 5;

    public string? LogKey { get; set; }
}
