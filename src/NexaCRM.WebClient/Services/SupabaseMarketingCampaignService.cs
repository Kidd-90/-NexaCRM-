using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.UI.Models;
using NexaCRM.UI.Models.Enums;
using NexaCRM.UI.Models.Supabase;
using NexaCRM.UI.Services.Interfaces;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using Task = System.Threading.Tasks.Task;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseMarketingCampaignService : IMarketingCampaignService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseMarketingCampaignService> _logger;

    public SupabaseMarketingCampaignService(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseMarketingCampaignService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<IEnumerable<MarketingCampaign>> GetCampaignsAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<MarketingCampaignRecord>()
                .Order(x => x.StartDate, PostgrestOrdering.Descending)
                .Get();

            var records = response.Models ?? new List<MarketingCampaignRecord>();
            if (records.Count == 0)
            {
                return Array.Empty<MarketingCampaign>();
            }

            return records.Select(MapToCampaign).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load marketing campaigns from Supabase.");
            throw;
        }
    }

    public async Task<MarketingCampaign?> GetCampaignByIdAsync(int id)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<MarketingCampaignRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Get();

            var record = response.Models.FirstOrDefault();
            return record is null ? null : MapToCampaign(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load marketing campaign {CampaignId} from Supabase.", id);
            throw;
        }
    }

    public async Task CreateCampaignAsync(MarketingCampaign campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = ToRecord(campaign);
            record.CreatedAt = DateTime.UtcNow;
            record.UpdatedAt = record.CreatedAt;

            await client.From<MarketingCampaignRecord>()
                .Insert(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create marketing campaign in Supabase.");
            throw;
        }
    }

    public async Task UpdateCampaignAsync(MarketingCampaign campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = ToRecord(campaign);
            record.UpdatedAt = DateTime.UtcNow;

            await client.From<MarketingCampaignRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, record.Id)
                .Update(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update marketing campaign {CampaignId} in Supabase.", campaign.Id);
            throw;
        }
    }

    public async Task DeleteCampaignAsync(int id)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            await client.From<MarketingCampaignRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Delete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete marketing campaign {CampaignId} from Supabase.", id);
            throw;
        }
    }

    private static MarketingCampaign MapToCampaign(MarketingCampaignRecord record)
    {
        return new MarketingCampaign
        {
            Id = record.Id,
            Name = record.Name,
            Type = record.Type,
            Status = ParseEnum(record.Status, CampaignStatus.Draft),
            StartDate = record.StartDate?.Date ?? DateTime.MinValue,
            EndDate = record.EndDate?.Date ?? DateTime.MinValue,
            Budget = record.Budget,
            ROI = record.Roi
        };
    }

    private static MarketingCampaignRecord ToRecord(MarketingCampaign campaign)
    {
        return new MarketingCampaignRecord
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Type = campaign.Type,
            Status = campaign.Status.ToString(),
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Budget = campaign.Budget,
            Roi = campaign.ROI
        };
    }

    private static CampaignStatus ParseEnum(string? value, CampaignStatus fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return Enum.TryParse(value, ignoreCase: true, out CampaignStatus parsed)
            ? parsed
            : fallback;
    }
}
