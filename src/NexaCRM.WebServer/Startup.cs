using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaCRM.Service.DependencyInjection;
using Supabase.Gotrue;
using NexaCRM.Service.Supabase;
using NexaCRM.Service.Supabase.Configuration;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.UI.Services;
using NexaCRM.UI.Services.Interfaces;
using NexaCRM.WebServer.Services;
using NexaCRM.Service.Supabase.Enterprise;

namespace NexaCRM.WebServer;

public sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddAuthorizationCore();

        services.AddRazorPages();
        services.AddServerSideBlazor(options =>
        {
            // Enable detailed errors in development for better debugging
            options.DetailedErrors = true;
        });

        services.AddNexaCrmAdminServices();

        services.AddSupabaseClientOptions(Configuration, validateOnStart: false);
        // In Development, use a singleton session persistence so an explicit
        // /test/signin request can persist a session that will be visible to
        // subsequently opened Blazor Server circuits. In non-development
        // environments keep the original scoped behaviour.
        var envName = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isDev = string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase);
        if (isDev)
        {
            services.AddSingleton<SupabaseServerSessionPersistence>();
        }
        else
        {
            services.AddScoped<SupabaseServerSessionPersistence>();
        }
        services.AddScoped<ISupabaseClientConfigurator, SupabaseServerClientConfigurator>();
        services.AddSupabaseClient();
        services.AddScoped<SupabaseAuthenticationStateProvider>(provider =>
        {
            var clientProvider = provider.GetRequiredService<SupabaseClientProvider>();
            var logger = provider.GetRequiredService<ILogger<SupabaseAuthenticationStateProvider>>();
            var options = provider.GetRequiredService<IOptions<SupabaseClientOptions>>();
            return new SupabaseAuthenticationStateProvider(clientProvider, logger, options);
        });
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SupabaseAuthenticationStateProvider>());
        services.AddScoped<NexaCRM.UI.Services.Interfaces.IAuthenticationService>(sp => sp.GetRequiredService<SupabaseAuthenticationStateProvider>());

        services.AddScoped<ActionInterop>();
        services.AddScoped<IMobileInteractionService, MobileInteractionService>();
        services.AddScoped<IGlobalActionService, GlobalActionService>();
        services.AddScoped<INavigationStateService, NavigationStateService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IUserFavoritesService, UserFavoritesService>();
        services.AddSingleton<SupabaseEnterpriseDataStore>();

        services.AddScoped<ISyncOrchestrationService, SupabaseSyncOrchestrationService>();
        services.AddScoped<ICommunicationHubService, SupabaseCommunicationHubService>();
        services.AddScoped<IFileHubService, SupabaseFileHubService>();
        services.AddScoped<ISettingsCustomizationService, SupabaseSettingsCustomizationService>();
        services.AddScoped<IUserGovernanceService, SupabaseUserGovernanceService>();

        services.AddScoped<IContactService, SupabaseContactService>();
        services.AddScoped<IDealService, SupabaseDealService>();
        services.AddScoped<ITaskService, SupabaseTaskService>();
        services.AddScoped<ISupportTicketService, SupabaseSupportTicketService>();
        services.AddScoped<IAgentService, SupabaseAgentService>();
        services.AddScoped<IMarketingCampaignService, SupabaseMarketingCampaignService>();
        services.AddScoped<IReportService, SupabaseReportService>();
        services.AddScoped<IActivityService, SupabaseActivityService>();
        services.AddScoped<ISalesManagementService, SupabaseSalesManagementService>();
        services.AddScoped<INoticeService, SupabaseNoticeService>();
        services.AddScoped<IEmailTemplateService, SupabaseEmailTemplateService>();
        services.AddScoped<ITeamService, SupabaseTeamService>();

    }

    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env,
        IServiceProvider services,
        IHostApplicationLifetime lifetime,
        ILogger<Startup> logger)
    {
        var showDetailedErrors = Configuration.GetValue<bool>("Hosting:ShowDetailedErrors");

        if (!env.IsDevelopment())
        {
            if (showDetailedErrors)
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = "text/html; charset=utf-8";

                        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                        var pathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                        if (exceptionFeature?.Error is not null)
                        {
                            logger.LogError(exceptionFeature.Error, "Unhandled exception while processing request for {Path}", pathFeature?.Path);

                            var builder = new StringBuilder()
                                .Append("<html><body style=\"font-family:monospace;padding:24px;\">")
                                .Append("<h1>Unhandled exception</h1>")
                                .Append("<p><strong>Path:</strong> ")
                                .Append(WebUtility.HtmlEncode(pathFeature?.Path ?? "(unknown)"))
                                .Append("</p><pre>")
                                .Append(WebUtility.HtmlEncode(exceptionFeature.Error.ToString()))
                                .Append("</pre></body></html>");

                            await context.Response.WriteAsync(builder.ToString()).ConfigureAwait(false);
                        }
                        else
                        {
                            await context.Response.WriteAsync("<html><body><h1>An unknown error occurred.</h1></body></html>").ConfigureAwait(false);
                        }
                    });
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        var forceHttps = Configuration.GetValue<bool>("Hosting:ForceHttps");
        if (!env.IsDevelopment() || forceHttps)
        {
            app.UseHttpsRedirection();
        }
        app.UseStaticFiles();

        // Development-only middleware: if the browser has an 'e2e_session' cookie
        // set by the E2E harness, trust it to build a simple ClaimsPrincipal so
        // server-side Blazor circuits will render as authenticated. This is only
        // enabled in Development and intended for test harnesses.
        if (env.IsDevelopment())
        {
            app.Use(async (context, next) =>
            {
                try
                {
                    if (context.Request.Cookies.TryGetValue("e2e_session", out var raw) && !string.IsNullOrWhiteSpace(raw))
                    {
                        try
                        {
                            // write raw cookie snapshot for debugging
                            try { System.IO.File.AppendAllText("/tmp/e2e_middleware_raw.log", raw + "\n"); } catch { }
                        }
                        catch { }
                        try
                        {
                            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(raw));
                            try { System.IO.File.AppendAllText("/tmp/e2e_middleware_decoded.log", json + "\n"); } catch { }
                            using var doc = System.Text.Json.JsonDocument.Parse(json);
                            var root = doc.RootElement;
                            var claims = new System.Collections.Generic.List<System.Security.Claims.Claim>();
                            string? parsedEmail = null;
                            var parsedRoles = new System.Collections.Generic.List<string>();
                            if (root.TryGetProperty("user", out var userEl))
                            {
                                if (userEl.TryGetProperty("id", out var idEl) && idEl.ValueKind == System.Text.Json.JsonValueKind.String)
                                {
                                        claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, idEl.GetString() ?? string.Empty));
                                }
                                if (userEl.TryGetProperty("email", out var emailEl) && emailEl.ValueKind == System.Text.Json.JsonValueKind.String)
                                {
                                    parsedEmail = emailEl.GetString();
                                        claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, parsedEmail ?? string.Empty));
                                        claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, parsedEmail ?? string.Empty));
                                }
                            }
                            if (root.TryGetProperty("roles", out var rolesEl) && rolesEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                            {
                                foreach (var r in rolesEl.EnumerateArray())
                                {
                                    if (r.ValueKind == System.Text.Json.JsonValueKind.String)
                                    {
                                        var role = r.GetString();
                                        if (!string.IsNullOrEmpty(role))
                                        {
                                            parsedRoles.Add(role);
                                            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
                                        }
                                    }
                                }
                            }

                            if (claims.Count > 0)
                            {
                                var identity = new System.Security.Claims.ClaimsIdentity(claims, "E2E");
                                context.User = new System.Security.Claims.ClaimsPrincipal(identity);
                                try
                                {
                                    logger.LogInformation("E2E auth cookie parsed: user={Email}, roles={Roles}", parsedEmail ?? "(none)", string.Join(',', parsedRoles));
                                }
                                catch { /* logging should never throw */ }
                                    try
                                    {
                                        // Also append a short file-based trace so CI/local test runs
                                        // can observe middleware activity from the test terminal.
                                        var txt = $"{DateTime.UtcNow:o} E2E_COOKIE user={parsedEmail ?? "(none)"} roles={string.Join(',', parsedRoles)}\n";
                                        System.IO.File.AppendAllText("/tmp/e2e_middleware.log", txt);
                                    }
                                    catch { }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            try { logger.LogDebug(ex, "Failed to parse e2e_session cookie JSON"); } catch { }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    try { logger.LogDebug(ex, "E2E middleware error"); } catch { }
                }

                await next();
            });
        }

        // Very early request logger: write a short trace of the request's
        // authentication state so E2E tests can verify whether the server
        // observed an authenticated principal on the initial GET.
        app.Use(async (context, next) =>
        {
            try
            {
                var isAuth = context.User?.Identity?.IsAuthenticated == true;
                var name = context.User?.Identity?.Name ?? "(none)";
                var roles = string.Join(',', context.User?.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value) ?? System.Array.Empty<string>());
                try
                {
                    logger.LogInformation("REQ_USER {Method} {Path} Auth={IsAuth} Name={Name} Roles={Roles}", context.Request.Method, context.Request.Path, isAuth, name, roles);
                }
                catch { }
                try
                {
                    System.IO.File.AppendAllText("/tmp/e2e_request_user.log", $"{DateTime.UtcNow:o} {context.Request.Method} {context.Request.Path} Auth={isAuth} Name={name} Roles={roles}\n");
                }
                catch { }
            }
            catch (Exception ex)
            {
                try { logger.LogDebug(ex, "Failed to emit request user trace"); } catch { }
            }

            await next();
        });

        app.UseRouting();

        app.UseAntiforgery();

        var schemeProvider = services.GetService<IAuthenticationSchemeProvider>();
        if (schemeProvider is not null)
        {
            var schemes = schemeProvider.GetAllSchemesAsync().GetAwaiter().GetResult();
            if (schemes.Any())
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }
            else
            {
                logger.LogInformation("No authentication schemes registered. Skipping UseAuthentication and UseAuthorization middleware.");
            }
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapBlazorHub();
            // Endpoint for receiving client console logs
            endpoints.MapPost("/client-logs", async context =>
            {
                try
                {
                    using var sr = new System.IO.StreamReader(context.Request.Body);
                    var body = await sr.ReadToEndAsync().ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        // Attempt to parse
                        try
                        {
                            var json = System.Text.Json.JsonDocument.Parse(body);
                            var root = json.RootElement;
                            var level = root.GetProperty("level").GetString() ?? "log";
                            var msg = root.GetProperty("message").GetString() ?? string.Empty;
                            var url = root.TryGetProperty("url", out var u) ? u.GetString() : null;
                            var ts = root.TryGetProperty("ts", out var t) ? t.GetString() : null;
                            var loggerFactory = context.RequestServices.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)) as Microsoft.Extensions.Logging.ILoggerFactory;
                            var log = loggerFactory?.CreateLogger("ClientConsole") ?? context.RequestServices.GetService(typeof(Microsoft.Extensions.Logging.ILogger<Startup>)) as Microsoft.Extensions.Logging.ILogger;
                            var message = $"[ClientConsole] {ts ?? ""} {level.ToUpperInvariant()} {url ?? ""} - {msg}";
                            if (level.Equals("error", StringComparison.OrdinalIgnoreCase))
                            {
                                log?.LogError(message);
                                Console.Error.WriteLine(message);
                            }
                            else if (level.Equals("warn", StringComparison.OrdinalIgnoreCase) || level.Equals("warning", StringComparison.OrdinalIgnoreCase))
                            {
                                log?.LogWarning(message);
                                Console.WriteLine(message);
                            }
                            else
                            {
                                log?.LogInformation(message);
                                Console.WriteLine(message);
                            }
                        }
                        catch (System.Text.Json.JsonException)
                        {
                            Console.WriteLine("[ClientConsole] Received non-json payload: " + body);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Failed to process client log: " + ex.Message);
                }

                context.Response.StatusCode = StatusCodes.Status204NoContent;
            });
            // Development-only test sign-in helper for E2E tests.
            // This endpoint is intentionally lightweight and only enabled in Development.
            if (env.IsDevelopment())
            {
                endpoints.MapPost("/test/signin", async context =>
                {
                    try
                    {
                        using var sr = new System.IO.StreamReader(context.Request.Body);
                        var body = await sr.ReadToEndAsync().ConfigureAwait(false);
                        if (string.IsNullOrWhiteSpace(body))
                        {
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await context.Response.WriteAsync("Missing body").ConfigureAwait(false);
                            return;
                        }

                        try
                        {
                            var json = System.Text.Json.JsonDocument.Parse(body);
                            var root = json.RootElement;
                            var username = root.TryGetProperty("username", out var u) ? u.GetString() : null;
                            var password = root.TryGetProperty("password", out var p) ? p.GetString() : null;

                            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                            {
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                await context.Response.WriteAsync("username and password required").ConfigureAwait(false);
                                return;
                            }

                            // For E2E tests we don't require a real authentication back-end.
                            // Accept any credentials in Development and return a small
                            // payload tests can use to seed client localStorage.
                            var roles = username.Contains("manager", StringComparison.OrdinalIgnoreCase) ? new[] { "manager" }
                                : username.Contains("sales", StringComparison.OrdinalIgnoreCase) ? new[] { "sales" }
                                : username.Contains("develop", StringComparison.OrdinalIgnoreCase) ? new[] { "developer" }
                                : new[] { "user" };

                            // Create a lightweight mock Supabase session payload so client-side
                            // SupabaseSessionPersistence can restore an authenticated session
                            // from localStorage for E2E tests. This is intentionally minimal
                            // and only used in Development for test-harness purposes.
                            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            // Create a typed Supabase Session and persist it to the server-side
                            // SupabaseServerSessionPersistence so server-side AuthenticationStateProvider
                            // will recognize the authenticated user in the Blazor Server circuits.
                            var session = new Session
                            {
                                AccessToken = Guid.NewGuid().ToString(),
                                TokenType = "bearer",
                                ExpiresIn = 3600,
                                RefreshToken = Guid.NewGuid().ToString(),
                                ProviderToken = null,
                                ProviderRefreshToken = null,
                                User = new User
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Email = username,
                                    Aud = "authenticated"
                                }
                            };

                            try
                            {
                                var persistence = context.RequestServices.GetService(typeof(SupabaseServerSessionPersistence)) as SupabaseServerSessionPersistence;
                                persistence?.SaveSession(session);
                            }
                            catch (Exception ex)
                            {
                                var lf = context.RequestServices.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)) as Microsoft.Extensions.Logging.ILoggerFactory;
                                lf?.CreateLogger("TestAuth")?.LogWarning(ex, "Failed to persist mock session to server session persistence.");
                            }

                            // Attempt to refresh the server Supabase client so its Auth.CurrentSession
                            // reflects the newly persisted session. This helps Blazor Server circuits
                            // that initialize after this request observe the mocked session.
                            try
                            {
                                var client = context.RequestServices.GetService(typeof(global::Supabase.Client)) as global::Supabase.Client;
                                if (client is not null)
                                {
                                    // RetrieveSessionAsync will consult the configured SessionHandler
                                    // (our SupabaseServerSessionPersistence) and update CurrentSession.
                                    await client.Auth.RetrieveSessionAsync().ConfigureAwait(false);
                                }
                            }
                            catch (Exception ex)
                            {
                                var lf2 = context.RequestServices.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)) as Microsoft.Extensions.Logging.ILoggerFactory;
                                lf2?.CreateLogger("TestAuth")?.LogDebug(ex, "Unable to refresh Supabase client session after persisting mock session.");
                            }

                            // Also return a JSON-shaped payload (snake_case) for client-side initialization
                            var mockSession = new
                            {
                                access_token = session.AccessToken,
                                token_type = session.TokenType,
                                expires_in = session.ExpiresIn,
                                expires_at = now + session.ExpiresIn,
                                refresh_token = session.RefreshToken,
                                provider_token = (string?)null,
                                provider_refresh_token = (string?)null,
                                user = new
                                {
                                    id = session.User.Id,
                                    email = session.User.Email,
                                    aud = session.User.Aud,
                                    confirmed_at = (string?)null
                                }
                            };

                            context.Response.StatusCode = StatusCodes.Status200OK;
                            context.Response.ContentType = "application/json; charset=utf-8";
                            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new { success = true, username, roles, session = mockSession })).ConfigureAwait(false);
                            return;
                        }
                        catch (System.Text.Json.JsonException)
                        {
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await context.Response.WriteAsync("Invalid JSON").ConfigureAwait(false);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        var loggerFactory = context.RequestServices.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)) as Microsoft.Extensions.Logging.ILoggerFactory;
                        var log = loggerFactory?.CreateLogger("TestAuth") as Microsoft.Extensions.Logging.ILogger;
                        log?.LogError(ex, "Test signin endpoint failed");
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsync("internal error").ConfigureAwait(false);
                    }
                })
                // The app registers antiforgery globally; allow this development-only
                // testing endpoint to skip antiforgery validation so tests can POST
                // without needing tokens or cookies.
                .WithMetadata(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());

                // Development-only trace endpoint to inspect the request principal.
                endpoints.MapGet("/test/trace", async context =>
                {
                    var isAuth = context.User?.Identity?.IsAuthenticated == true;
                    var name = context.User?.Identity?.Name;
                    var roles = context.User?.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray() ?? Array.Empty<string>();
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new { isAuthenticated = isAuth, name, roles }));
                });
            }
            endpoints.MapFallbackToPage("/_Host");
        });

        var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
        lifetime.ApplicationStarted.Register(() =>
        {
            _ = StartDuplicateMonitorAsync(scopeFactory, logger);
        });

        ValidateAdminServices(app.ApplicationServices, logger);
    }

    private static void ValidateAdminServices(IServiceProvider serviceProvider, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(logger);

        using var scope = serviceProvider.CreateScope();
        try
        {
            var adminService = scope.ServiceProvider.GetRequiredService<IDbAdminService>();
            _ = scope.ServiceProvider.GetRequiredService<IDbDataService>();
            _ = scope.ServiceProvider.GetRequiredService<IDuplicateService>();
            _ = scope.ServiceProvider.GetRequiredService<INotificationFeedService>();

            logger.LogInformation(
                "Db admin services resolved successfully for WebServer host using {AdminService}.",
                adminService.GetType().FullName);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to resolve Db admin service dependencies during WebServer startup.");
            throw new InvalidOperationException("Db admin services are not configured correctly for WebServer.", ex);
        }
    }

    private static async Task StartDuplicateMonitorAsync(IServiceScopeFactory scopeFactory, ILogger logger)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var monitor = scope.ServiceProvider.GetRequiredService<IDuplicateMonitorService>();
            await monitor.StartAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start the duplicate monitor service.");
        }
    }
}
