using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NexaCRM.UI.Services.Interfaces;

public enum GlobalActionType
{
    Call,
    Email,
    ScheduleMeeting,
    AddContact,
    AddDeal
}

public sealed record GlobalActionRequest(
    GlobalActionType Type,
    string? TargetId = null,
    string? TargetName = null,
    string? PhoneNumber = null,
    string? Email = null,
    IReadOnlyDictionary<string, string>? Metadata = null);

public enum GlobalActionResult
{
    Completed,
    Cancelled,
    NotHandled,
    Failed
}

public sealed class GlobalActionContext
{
    private readonly TaskCompletionSource<GlobalActionResult> _completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public GlobalActionContext(GlobalActionRequest request)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }

    public GlobalActionRequest Request { get; }

    public Task<GlobalActionResult> Completion => _completionSource.Task;

    public void Complete(GlobalActionResult result) => _completionSource.TrySetResult(result);

    public void Fail() => _completionSource.TrySetResult(GlobalActionResult.Failed);

    public void Cancel() => _completionSource.TrySetResult(GlobalActionResult.Cancelled);
}

public interface IGlobalActionService
{
    event Func<GlobalActionContext, Task>? ActionRequested;

    Task<GlobalActionResult> LaunchAsync(GlobalActionRequest request, CancellationToken cancellationToken = default);
}
