using System.ComponentModel.DataAnnotations;

namespace NexaCRM.WebClient.Models.Settings;

public class UserProfile
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? ProfilePicture { get; set; }
}

