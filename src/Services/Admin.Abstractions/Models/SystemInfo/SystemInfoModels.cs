namespace NexaCRM.Services.Admin.Models.SystemInfo;

public record SystemInfo(
    string Terms = "",
    string CompanyAddress = "",
    string[] SupportContacts = null!
);

