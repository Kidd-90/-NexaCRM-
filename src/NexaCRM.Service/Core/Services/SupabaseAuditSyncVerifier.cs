using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.SupabaseOperations;

namespace NexaCRM.Services.Admin;

public sealed class SupabaseAuditSyncVerifier : ISupabaseAuditSyncVerifier
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseServerOptions _serverOptions;
    private readonly SupabaseAuditSyncOptions _auditOptions;
    private readonly ILogger<SupabaseAuditSyncVerifier> _logger;

    public SupabaseAuditSyncVerifier(
        HttpClient httpClient,
        IOptions<SupabaseServerOptions> serverOptions,
        IOptions<SupabaseAuditSyncOptions> auditOptions,
        ILogger<SupabaseAuditSyncVerifier> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _serverOptions = serverOptions?.Value ?? throw new ArgumentNullException(nameof(serverOptions));
        _serverOptions.Validate();
        _auditOptions = auditOptions?.Value ?? throw new ArgumentNullException(nameof(auditOptions));
        _auditOptions.Validate();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SupabaseAuditSyncReport> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var issues = new List<string>();
        var since = DateTimeOffset.UtcNow - TimeSpan.FromMinutes(_auditOptions.WindowMinutes);

        var auditCount = await CountTableAsync("audit_logs", since, cancellationToken).ConfigureAwait(false);
        var integrationCount = await CountTableAsync("integration_events", since, cancellationToken).ConfigureAwait(false);

        if (auditCount == 0)
        {
            issues.Add("No audit log entries recorded within the validation window.");
        }

        if (integrationCount == 0)
        {
            issues.Add("No integration events recorded within the validation window.");
        }

        var drift = Math.Abs(auditCount - integrationCount);
        if (drift > _auditOptions.AllowedDriftCount)
        {
            issues.Add($"Audit/integration event drift exceeded threshold: {drift} > {_auditOptions.AllowedDriftCount}.");
        }

        var isConsistent = issues.Count == 0;

        return new SupabaseAuditSyncReport(
            isConsistent,
            auditCount,
            integrationCount,
            issues,
            DateTimeOffset.UtcNow);
    }

    private async Task<int> CountTableAsync(string table, DateTimeOffset since, CancellationToken cancellationToken)
    {
        var baseUrl = _serverOptions.Url?.TrimEnd('/') ?? throw new InvalidOperationException("Supabase URL is required.");
        var requestUri = $"{baseUrl}/rest/v1/{table}?select=id&created_at=gte.{Uri.EscapeDataString(since.ToString("O", CultureInfo.InvariantCulture))}";

        using var request = new HttpRequestMessage(HttpMethod.Head, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serverOptions.ServiceRoleKey);
        request.Headers.Add("apikey", _serverOptions.ServiceRoleKey);
        request.Headers.Add("Prefer", "count=exact");

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Supabase count request for {Table} failed with {Status}.", table, response.StatusCode);
                return 0;
            }

            if (response.Headers.TryGetValues("Content-Range", out var ranges))
            {
                var headerValue = ranges.FirstOrDefault();
                if (TryParseCount(headerValue, out var count))
                {
                    return count;
                }
            }

            _logger.LogWarning("Supabase count request for {Table} returned no Content-Range header.", table);
            return 0;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Failed to count Supabase table {Table}.", table);
            return 0;
        }
    }

    private static bool TryParseCount(string? contentRange, out int count)
    {
        count = 0;
        if (string.IsNullOrWhiteSpace(contentRange))
        {
            return false;
        }

        var parts = contentRange.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        var totalPart = parts[1];
        if (totalPart == "*")
        {
            return false;
        }

        return int.TryParse(totalPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out count);
    }
}

