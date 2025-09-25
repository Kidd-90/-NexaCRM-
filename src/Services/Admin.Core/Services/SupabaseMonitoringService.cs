using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.SupabaseOperations;

namespace NexaCRM.Services.Admin;

public sealed class SupabaseMonitoringService : ISupabaseMonitoringService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IOptions<SupabaseMonitoringOptions> _options;
    private readonly ILogger<SupabaseMonitoringService> _logger;

    public SupabaseMonitoringService(
        HttpClient httpClient,
        IOptions<SupabaseMonitoringOptions> options,
        ILogger<SupabaseMonitoringService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SupabaseMetricSnapshot> GetCurrentMetricsAsync(CancellationToken cancellationToken = default)
    {
        var settings = _options.Value;
        settings.Validate();

        var requestUri = BuildRequestUri(settings);
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Supabase metrics request failed with status {StatusCode}.",
                    response.StatusCode);
                return SupabaseMetricSnapshot.Empty(DateTimeOffset.UtcNow);
            }

            var payload = await response.Content
                .ReadFromJsonAsync<MetricsEnvelope>(SerializerOptions, cancellationToken)
                .ConfigureAwait(false);

            if (payload?.Series is null || payload.Series.Count == 0)
            {
                _logger.LogWarning("Supabase metrics payload was empty.");
                return SupabaseMetricSnapshot.Empty(DateTimeOffset.UtcNow);
            }

            var snapshot = new SupabaseMetricSnapshot(
                DateTimeOffset.UtcNow,
                payload.Series
                    .Select(MapSeries)
                    .Where(series => series.Points.Count > 0)
                    .ToArray());

            return snapshot;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Failed to retrieve Supabase metrics.");
            return SupabaseMetricSnapshot.Empty(DateTimeOffset.UtcNow);
        }
    }

    private static string BuildRequestUri(SupabaseMonitoringOptions options)
    {
        var baseUrl = options.ApiBaseUrl?.TrimEnd('/') ?? string.Empty;
        var projectRef = options.ProjectRef ?? throw new InvalidOperationException("Supabase project ref must be configured.");
        var metricKeys = options.MetricKeys ?? Array.Empty<string>();
        var joinedKeys = string.Join(',', metricKeys.Select(Uri.EscapeDataString));
        var window = options.WindowMinutes.ToString(CultureInfo.InvariantCulture);
        return $"{baseUrl}/{projectRef}/metrics?window={window}&metric_keys={joinedKeys}";
    }

    private static SupabaseMetricSeries MapSeries(KeyValuePair<string, MetricSeries> kvp)
    {
        var points = kvp.Value.Data?.Select(point =>
                new SupabaseMetricPoint(point.Timestamp, point.Value))
            .ToArray() ?? Array.Empty<SupabaseMetricPoint>();

        return new SupabaseMetricSeries(kvp.Key, kvp.Value.Unit ?? string.Empty, points);
    }

    private sealed record MetricsEnvelope(Dictionary<string, MetricSeries> Series);

    private sealed record MetricSeries(string? Unit, IReadOnlyList<MetricPoint>? Data);

    private sealed record MetricPoint(DateTimeOffset Timestamp, decimal Value);
}

