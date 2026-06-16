using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CoreMS.AvailabilityChecks.ServiceDefaults;

public static class OTel
{
    public static string ApplicationName { get; set; } = "MrJB.AvailabilityChecks";

    public static string ServiceVersion { get; set; } = "1.0.0";

    public static string BasePath { get; set; } = "mrjamiebowman.availabilitychecks";

    public static readonly ActivitySource ActivitySource = new ActivitySource(ApplicationName);

    public static class MetricNames
    {

    }

    public static class Meters
    {
        public static Meter Meter = new Meter(ApplicationName, "1.0.0");

        public static readonly Counter<long> AvailabilityCheck = Meter.CreateCounter<long>("availability_check");

        public static readonly Histogram<double> AvailabilityDurationMs = Meter.CreateHistogram<double>("availability_check_duration_ms");

        public static class Names
        {
            public static string UserKnownIssues = $"{BasePath}.check";
        }

        public static void AddAvailabilityCheck(int c = 1, TagList tagList = default) => AvailabilityCheck.Add(c, tagList);
    }
}
