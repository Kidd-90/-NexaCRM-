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

public record SmsSettings(
    IList<string> SenderNumbers,
    IList<string> Templates
)
{
    public SmsSettings() : this(new List<string>(), new List<string>()) { }
}

