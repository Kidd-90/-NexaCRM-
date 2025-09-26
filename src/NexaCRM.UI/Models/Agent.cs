namespace NexaCRM.UI.Models;

/// <summary>
/// Represents an agent displayed within the Blazor client experience.
/// </summary>
public sealed class Agent
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string Role { get; set; } = string.Empty;
}
