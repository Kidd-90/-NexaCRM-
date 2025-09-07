// Program.cs
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NexaCRM.WebClient;
using Microsoft.AspNetCore.Components.Authorization;
using NexaCRM.WebClient.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// App ������Ʈ ����Ʈ ���� ����
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient ���: BaseAddress�� ȣ��Ʈ ȯ�� �ּҷ� ����
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();


await builder.Build().RunAsync();
