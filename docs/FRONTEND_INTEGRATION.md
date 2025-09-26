# Guide: Integrating Supabase with the NexaCRM Blazor WebClient

This guide provides instructions and code examples for connecting the `NexaCRM.WebClient` application to your new Supabase backend.

## Step 1: Add the Supabase Client Library

First, you need to add the `supabase-csharp` library to your Blazor project. Open a terminal in the `src/Web/NexaCRM.WebClient` directory and run the following command:

```bash
dotnet add package supabase-csharp
```

This will add the necessary NuGet package to your `NexaCRM.WebClient.csproj` file.

## Step 2: Initialize the Supabase Client

You need to initialize the Supabase client with your project's URL and Anon Key. You can find these in your Supabase project settings under "API". It's best practice to store these in your `appsettings.json` file.

**1. Add to `wwwroot/appsettings.json`:**
```json
{
  "Supabase": {
    "Url": "YOUR_SUPABASE_URL",
    "AnonKey": "YOUR_SUPABASE_ANON_KEY"
  }
}
```
**Important:** The Anon Key is safe to expose in a browser client. It works with your Row-Level Security policies to allow access.

**2. Register the client in `Program.cs`:**
Modify your `src/Web/NexaCRM.WebClient/Program.cs` file to register the Supabase client as a singleton service.

```csharp
// using Supabase; // Add this using statement

// ... other using statements

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ... other services

// Add Supabase client registration
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseAnonKey = builder.Configuration["Supabase:AnonKey"];

builder.Services.AddSingleton(new Client(supabaseUrl, supabaseAnonKey, new Supabase.SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = true
}));


await builder.Build().RunAsync();
```

## Step 3: Define C# Models

Create C# classes that match the database tables you created. The `supabase-csharp` library uses these models to map data. You'll need to install `Postgrest.Attributes` which comes with `supabase-csharp`.

Here is an example for `Deal.cs`. You would create similar models for `Contact`, `Company`, etc.

**`Models/Deal.cs`:**
```csharp
using Postgrest.Attributes;
using Postgrest.Models;

namespace NexaCRM.WebClient.Models
{
    [Table("deals")]
    public class Deal : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("value")]
        public decimal Value { get; set; }

        [Column("stage_id")]
        public int StageId { get; set; }

        [Column("assigned_to")]
        public Guid AssignedTo { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
```

## Step 4: Authentication

You can now handle user authentication by calling the Supabase client from your Blazor components.

**Example `Login.razor`:**
```csharp
@page "/login"
@inject Supabase.Client SupabaseClient
@inject NavigationManager Navigation

<h3>Login</h3>

<input @bind="email" placeholder="Email" />
<input @bind="password" type="password" placeholder="Password" />
<button @onclick="HandleLogin">Log In</button>
<div>@errorMessage</div>

@code {
    private string email;
    private string password;
    private string errorMessage;

    private async Task HandleLogin()
    {
        try
        {
            var session = await SupabaseClient.Auth.SignIn(email, password);
            if (session != null)
            {
                Navigation.NavigateTo("/");
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Login failed: {ex.Message}";
        }
    }
}
```

## Step 5: Data Operations (CRUD)

Create services to encapsulate your data logic. These services will use the injected Supabase client.

**Example `Services/DealService.cs`:**
```csharp
using NexaCRM.WebClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services
{
    public class DealService
    {
        private readonly Supabase.Client _supabase;

        public DealService(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public async Task<List<Deal>> GetDeals()
        {
            // The RLS policy ensures users only get their own deals.
            var response = await _supabase.From<Deal>().Get();
            return response.Models;
        }

        public async Task<Deal> CreateDeal(Deal newDeal)
        {
            newDeal.AssignedTo = _supabase.Auth.CurrentUser.Id; // Assign to current user
            var response = await _supabase.From<Deal>().Insert(newDeal);
            return response.Models.FirstOrDefault();
        }
    }
}
```
You would then register this service in `Program.cs`: `builder.Services.AddScoped<DealService>();`

## Step 6: Real-time Subscriptions

Supabase makes real-time updates easy. You can listen for changes in the database and react to them in your UI.

**Example in a Blazor component (`DealsKanban.razor`):**
```csharp
@implements IAsyncDisposable
@inject Supabase.Client SupabaseClient

// ... UI for the Kanban board ...

@code {
    private Supabase.Realtime.Channel dealChannel;

    protected override async Task OnInitializedAsync()
    {
        // Subscribe to changes on the 'deals' table
        dealChannel = SupabaseClient.Realtime.Channel("deals");

        dealChannel.On(Supabase.Realtime.Channel.ListenType.Broadcast, "deal_updated", (sender, args) =>
        {
            var payload = (JsonElement)args[0];
            // Handle the updated deal payload
            // For example, find and update the deal in your local list
            InvokeAsync(StateHasChanged);
        });

        await dealChannel.Subscribe();
    }

    public async ValueTask DisposeAsync()
    {
        if (dealChannel != null)
        {
            await dealChannel.Unsubscribe();
        }
    }
}
```
**Note:** For this to work, you need to enable the "Broadcast" feature for your table in the Supabase dashboard (`Database -> Replication`).

This guide provides the foundational steps to replace your existing backend services with a modern, simplified Supabase architecture.
