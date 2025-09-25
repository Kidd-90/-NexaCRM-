using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BlazorWebApp.Tests.Infrastructure;
using BuildingBlocks.Common.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NexaCRM.Services.Admin;
using NexaCRM.Services.Admin.Models.SupabaseOperations;
using Xunit;

namespace BlazorWebApp.Tests;

public sealed class SupabaseMonitoringServiceTests
{
    [Fact]
    public async Task GetCurrentMetricsAsync_ReturnsSnapshot_WhenResponseIsSuccessful()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            var payload = new
            {
                series = new
                {
                    db_connections = new
                    {
                        unit = "connections",
                        data = new[]
                        {
                            new { timestamp = DateTimeOffset.UtcNow, value = 12.5m }
                        }
                    }
                }
            };
            var json = JsonSerializer.Serialize(payload);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            });
        });

        var options = Options.Create(new SupabaseMonitoringOptions
        {
            ApiBaseUrl = "https://api.supabase.com/v1/projects",
            ProjectRef = "test-project",
            AccessToken = "token",
            MetricKeys = new[] { "db_connections" },
            WindowMinutes = 15
        });

        var service = new SupabaseMonitoringService(new HttpClient(handler), options, NullLogger<SupabaseMonitoringService>.Instance);

        SupabaseMetricSnapshot snapshot = await service.GetCurrentMetricsAsync();

        Assert.NotNull(snapshot);
        Assert.NotEmpty(snapshot.Series);
        Assert.Equal("db_connections", snapshot.Series[0].Key);
        Assert.Single(snapshot.Series[0].Points);
        Assert.Equal(12.5m, snapshot.Series[0].Points[0].Value);
    }

    [Fact]
    public async Task GetCurrentMetricsAsync_ReturnsEmpty_WhenApiFails()
    {
        var handler = new StubHttpMessageHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var options = Options.Create(new SupabaseMonitoringOptions
        {
            ApiBaseUrl = "https://api.supabase.com/v1/projects",
            ProjectRef = "test-project",
            AccessToken = "token"
        });

        var service = new SupabaseMonitoringService(new HttpClient(handler), options, NullLogger<SupabaseMonitoringService>.Instance);

        SupabaseMetricSnapshot snapshot = await service.GetCurrentMetricsAsync();

        Assert.Empty(snapshot.Series);
    }
}

