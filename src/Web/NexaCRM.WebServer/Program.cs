using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NexaCRM.WebClient;
using NexaCRM.WebClient.DependencyInjection;
using NexaCRM.WebClient.Options;
using NexaCRM.WebServer.Services;
using Supabase.Gotrue;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".NexaCRM.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services
    .AddNexaCrmCoreServices(builder.Configuration)
    .AddNexaCrmServerRuntime(options =>
    {
        options.HttpClientFactory = sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return factory.CreateClient("NexaCRM.Api");
        };
        options.SessionPersistenceFactory = sp => sp.GetRequiredService<ServerSupabaseSessionPersistence>();
        options.SupabaseClientFactory = sp =>
        {
            var supabaseOptions = sp.GetRequiredService<IOptions<SupabaseClientOptions>>().Value;
            if (string.IsNullOrWhiteSpace(supabaseOptions.Url) || string.IsNullOrWhiteSpace(supabaseOptions.AnonKey))
            {
                throw new InvalidOperationException("Supabase configuration must include Url and AnonKey.");
            }

            return new Supabase.Client(supabaseOptions.Url, supabaseOptions.AnonKey);
        };
        options.ConfigureServices = services =>
        {
            services.AddScoped<ServerSupabaseSessionPersistence>();
            services.AddHttpClient("NexaCRM.Api", (sp, client) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var baseUrl = configuration["ApiGateway:BaseUrl"];
                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            });
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
