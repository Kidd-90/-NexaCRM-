using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Db;

namespace NexaCRM.Pages.Pages.Db.Shared;

public abstract class DbCustomerListPageBase : ComponentBase
{
    [Inject]
    protected IDbDataService DbDataService { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected IReadOnlyList<DbCustomer> Customers { get; private set; } = Array.Empty<DbCustomer>();

    protected bool IsLoading { get; private set; }

    protected string? ErrorMessage { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        await RefreshAsync();
    }

    protected async Task RefreshAsync()
    {
        IsLoading = true;
        SetError(null);

        List<DbCustomer> snapshot = new();

        try
        {
            var result = await FetchCustomersAsync();
            snapshot = result?.ToList() ?? new List<DbCustomer>();
        }
        catch (Exception ex)
        {
            SetError("데이터를 불러오지 못했습니다. 잠시 후 다시 시도해주세요.");
            Console.WriteLine($"[{GetType().Name}] Failed to load DB customers: {ex}");
        }

        Customers = snapshot;
        await OnDataLoadedAsync(Customers);

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    protected void SetError(string? message) => ErrorMessage = message;

    protected abstract Task<IEnumerable<DbCustomer>> FetchCustomersAsync();

    protected virtual Task OnDataLoadedAsync(IReadOnlyList<DbCustomer> customers) => Task.CompletedTask;
}
