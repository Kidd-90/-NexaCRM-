using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorWebApp.Tests.Infrastructure;
using BuildingBlocks.Common.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NexaCRM.Services.Admin;
using Xunit;

namespace BlazorWebApp.Tests;

public sealed class SupabaseAuditSyncVerifierTests
{
    [Fact]
    public async Task ValidateAsync_ReturnsConsistent_WhenCountsMatch()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Head, request.Method);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("Content-Range", "0-0/10");
            return Task.FromResult(response);
        });

        var serverOptions = Options.Create(new SupabaseServerOptions
        {
            Url = "https://example.supabase.co",
            ServiceRoleKey = "service-role",
            JwtSecret = "secret"
        });

        var auditOptions = Options.Create(new SupabaseAuditSyncOptions
        {
            AllowedDriftCount = 5,
            WindowMinutes = 60
        });

        var verifier = new SupabaseAuditSyncVerifier(
            new HttpClient(handler),
            serverOptions,
            auditOptions,
            NullLogger<SupabaseAuditSyncVerifier>.Instance);

        var report = await verifier.ValidateAsync();

        Assert.True(report.IsConsistent);
        Assert.Equal(10, report.AuditLogCount);
        Assert.Equal(10, report.IntegrationEventCount);
        Assert.Empty(report.Issues);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsIssues_WhenDriftExceedsThreshold()
    {
        var callCount = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("Content-Range", callCount == 0 ? "0-0/25" : "0-0/10");
            callCount++;
            return Task.FromResult(response);
        });

        var serverOptions = Options.Create(new SupabaseServerOptions
        {
            Url = "https://example.supabase.co",
            ServiceRoleKey = "service-role",
            JwtSecret = "secret"
        });

        var auditOptions = Options.Create(new SupabaseAuditSyncOptions
        {
            AllowedDriftCount = 5,
            WindowMinutes = 60
        });

        var verifier = new SupabaseAuditSyncVerifier(
            new HttpClient(handler),
            serverOptions,
            auditOptions,
            NullLogger<SupabaseAuditSyncVerifier>.Instance);

        var report = await verifier.ValidateAsync();

        Assert.False(report.IsConsistent);
        Assert.NotEmpty(report.Issues);
    }
}

