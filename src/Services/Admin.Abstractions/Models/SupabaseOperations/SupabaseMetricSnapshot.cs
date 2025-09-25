using System;
using System.Collections.Generic;

namespace NexaCRM.Services.Admin.Models.SupabaseOperations;

public sealed record SupabaseMetricPoint(DateTimeOffset Timestamp, decimal Value);

public sealed record SupabaseMetricSeries(string Key, string Unit, IReadOnlyList<SupabaseMetricPoint> Points);

public sealed record SupabaseMetricSnapshot(DateTimeOffset CollectedAt, IReadOnlyList<SupabaseMetricSeries> Series)
{
    public static SupabaseMetricSnapshot Empty(DateTimeOffset collectedAt) =>
        new(collectedAt, Array.Empty<SupabaseMetricSeries>());
}

