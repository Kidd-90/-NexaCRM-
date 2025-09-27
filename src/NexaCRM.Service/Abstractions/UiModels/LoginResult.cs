namespace NexaCRM.UI.Models;

public enum LoginFailureReason
{
    None = 0,
    MissingUsername,
    MissingPassword,
    UserNotFound,
    InvalidPassword,
    RequiresApproval,
    Unknown
}

public readonly record struct LoginResult(bool Succeeded, LoginFailureReason FailureReason, string? ErrorMessage)
{
    public static LoginResult Success() => new(true, LoginFailureReason.None, null);

    public static LoginResult Failed(LoginFailureReason reason, string? message) => new(false, reason, message);
}
