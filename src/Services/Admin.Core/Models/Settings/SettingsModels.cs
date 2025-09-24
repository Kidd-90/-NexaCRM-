using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NexaCRM.WebClient.Models.Settings;

public record CompanyInfo(
    string Name = "",
    string Address = "",
    string ContactNumber = ""
);

public record SecuritySettings(
    bool IpRestrictionEnabled = false,
    bool LoginBlockEnabled = false,
    IList<string> IpWhitelist = null!,
    [property: Range(1, 10)] int MaxLoginAttempts = 5,
    [property: Range(1, 1440)] int BlockDurationMinutes = 15
)
{
    public SecuritySettings() : this(false, false, new List<string>(), 5, 15) { }
}

public class SmsSettings
{
    public IList<string> SenderNumbers { get; set; }
    public IList<string> Templates { get; set; }

    [Required]
    public string ProviderApiKey { get; set; } = string.Empty;

    [Required]
    public string ProviderApiSecret { get; set; } = string.Empty;

    public string DefaultTemplate { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[a-zA-Z0-9]{3,11}$", ErrorMessage = "Sender ID must be alphanumeric and 3-11 characters.")]
    public string SenderId { get; set; } = string.Empty;

    public SmsSettings()
    {
        SenderNumbers = new List<string>();
        Templates = new List<string>();
    }
}

