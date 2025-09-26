using System;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.WebClient.Services;

public sealed class GlobalActionService : IGlobalActionService
{
    public event Func<GlobalActionContext, Task>? ActionRequested;

    public async Task<GlobalActionResult> LaunchAsync(GlobalActionRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var handler = ActionRequested;
        if (handler is null)
        {
            return GlobalActionResult.NotHandled;
        }

        var context = new GlobalActionContext(request);
        var invocation = handler(context);

        using (cancellationToken.Register(context.Cancel))
        {
            await invocation.ConfigureAwait(false);
            return await context.Completion.ConfigureAwait(false);
        }
    }
}
