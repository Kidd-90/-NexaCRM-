namespace NexaCRM.WebClient.Models.CustomerCenter;

using System.ComponentModel.DataAnnotations;

public record Notice(int Id, string Title, string Content);

public class FaqItem
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Question { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Answer { get; set; } = string.Empty;

    public int Order { get; set; }
}

