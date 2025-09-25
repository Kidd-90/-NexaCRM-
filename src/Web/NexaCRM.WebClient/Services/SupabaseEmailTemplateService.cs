using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Services.Interfaces;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseEmailTemplateService : IEmailTemplateService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseEmailTemplateService> _logger;

    public SupabaseEmailTemplateService(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseEmailTemplateService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task SaveTemplateAsync(EmailTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            if (template.Id == Guid.Empty)
            {
                template.Id = Guid.NewGuid();
            }

            var now = DateTime.UtcNow;
            var templateRecord = new EmailTemplateRecord
            {
                Id = template.Id,
                Subject = template.Subject,
                UpdatedAt = now,
                CreatedAt = now
            };

            await client.From<EmailTemplateRecord>()
                .Upsert(templateRecord);

            await client.From<EmailBlockRecord>()
                .Filter(x => x.TemplateId, PostgrestOperator.Equals, template.Id)
                .Delete();

            if (template.Blocks.Count == 0)
            {
                return;
            }

            var orderedBlocks = template.Blocks
                .Select((block, index) => new EmailBlockRecord
                {
                    TemplateId = template.Id,
                    BlockOrder = index,
                    BlockType = block.Type,
                    Content = block.Content
                })
                .ToList();

            await client.From<EmailBlockRecord>()
                .Insert(orderedBlocks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save email template {TemplateId} to Supabase.", template.Id);
            throw;
        }
    }

    public async Task<EmailTemplate?> LoadTemplateAsync(Guid id)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var templateResponse = await client.From<EmailTemplateRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Get();

            var templateRecord = templateResponse.Models.FirstOrDefault();
            if (templateRecord is null)
            {
                return null;
            }

            var blockResponse = await client.From<EmailBlockRecord>()
                .Filter(x => x.TemplateId, PostgrestOperator.Equals, id)
                .Order(x => x.BlockOrder, PostgrestOrdering.Ascending)
                .Get();

            var blocks = blockResponse.Models ?? new List<EmailBlockRecord>();

            return new EmailTemplate
            {
                Id = templateRecord.Id,
                Subject = templateRecord.Subject,
                Blocks = blocks.Select(block => new EmailBlock
                {
                    Type = block.BlockType,
                    Content = block.Content
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load email template {TemplateId} from Supabase.", id);
            throw;
        }
    }
}
