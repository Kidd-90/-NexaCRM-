// Program.cs
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NexaCRM.WebClient;
using Microsoft.AspNetCore.Components.Authorization;
using NexaCRM.WebClient.Services;
using NexaCRM.WebClient.Services.Interfaces;
using NexaCRM.WebClient.Services.Mock;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// App 컴포넌트 마운트 지점 설정
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient 등록: BaseAddress를 호스트 환경 주소로 설정
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
);

builder.Services.AddAuthorizationCore();
builder.Services.AddLocalization(options => { options.ResourcesPath = "Resources"; });
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<IContactService, MockContactService>();
builder.Services.AddScoped<IDealService, MockDealService>();
builder.Services.AddScoped<ITaskService, MockTaskService>();
builder.Services.AddScoped<ISupportTicketService, MockSupportTicketService>();
builder.Services.AddScoped<IAgentService, MockAgentService>();
builder.Services.AddScoped<IMarketingCampaignService, MockMarketingCampaignService>();
builder.Services.AddScoped<IReportService, MockReportService>();
builder.Services.AddScoped<IActivityService, MockActivityService>();

using System.Globalization;

var culture = new CultureInfo("ko-KR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await builder.Build().RunAsync();
