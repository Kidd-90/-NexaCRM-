namespace NexaCRM.WebClient.Models.SystemInfo;

public record SystemInfo(
    string Terms = "",
    string CompanyAddress = "",
    string[] SupportContacts = null!
);

