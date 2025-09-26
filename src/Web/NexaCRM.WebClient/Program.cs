// Program.cs
using System;
using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.WebClient;
using NexaCRM.WebClient.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add root components to the app
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddNexaCrmCoreServices(builder.Configuration)
    .AddNexaCrmWebAssemblyRuntime(builder);

var culture = new CultureInfo("ko-KR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var host = builder.Build();

await StartDuplicateMonitorAsync(host.Services);
await host.RunAsync();

static async Task StartDuplicateMonitorAsync(IServiceProvider services)
{
    try
    {
        var monitor = services.GetRequiredService<IDuplicateMonitorService>();
        await monitor.StartAsync();
    }
    catch (Exception ex)
    {
        var loggerFactory = services.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("Startup");
        logger?.LogError(ex, "Failed to start the duplicate monitor service.");
    }
}
