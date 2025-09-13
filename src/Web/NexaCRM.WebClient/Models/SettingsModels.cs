using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.Settings;

public record CompanyInfo(
    string Name = "",
    string Address = "",
    string ContactNumber = ""
);

public record SecuritySettings(
    bool IpRestrictionEnabled = false,
    bool LoginBlockEnabled = false
);

public record SmsSettings(
    IList<string> SenderNumbers,
    IList<string> Templates
)
{
    public SmsSettings() : this(new List<string>(), new List<string>()) { }
}

