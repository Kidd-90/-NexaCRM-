using System;
using System.Threading.Tasks;
using NexaCRM.UI.Models;

namespace NexaCRM.UI.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        System.Threading.Tasks.Task SaveTemplateAsync(EmailTemplate template);
        System.Threading.Tasks.Task<EmailTemplate?> LoadTemplateAsync(Guid id);
    }
}
