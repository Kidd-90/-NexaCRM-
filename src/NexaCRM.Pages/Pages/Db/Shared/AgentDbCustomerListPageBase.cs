using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace NexaCRM.Pages.Pages.Db.Shared;

public abstract class AgentDbCustomerListPageBase : DbCustomerListPageBase
{
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    protected async Task<string?> ResolveAgentNameAsync()
    {
        if (AuthenticationStateTask is null)
        {
            return null;
        }

        var authState = await AuthenticationStateTask;
        var user = authState.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        if (user.IsInRole("Manager"))
        {
            return "김관리";
        }

        if (user.IsInRole("Sales"))
        {
            return "이영업";
        }

        return string.IsNullOrWhiteSpace(user.Identity?.Name)
            ? "이영업"
            : user.Identity!.Name;
    }
}
