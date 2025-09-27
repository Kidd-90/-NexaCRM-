using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.UI.Models;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.UI.Services.Mock
{
    public class MockEmailTemplateService : IEmailTemplateService
    {
        private readonly List<EmailTemplate> _templates = new();

        public System.Threading.Tasks.Task SaveTemplateAsync(EmailTemplate template)
        {
            var existing = _templates.FirstOrDefault(t => t.Id == template.Id);
            if (existing != null)
            {
                existing.Subject = template.Subject;
                existing.Blocks = template.Blocks;
            }
            else
            {
                _templates.Add(template);
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task<EmailTemplate?> LoadTemplateAsync(Guid id)
        {
            var template = _templates.FirstOrDefault(t => t.Id == id);
            return System.Threading.Tasks.Task.FromResult(template);
        }
    }
}
