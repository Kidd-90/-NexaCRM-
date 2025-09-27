using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.UI.Models;
using NexaCRM.UI.Models.Supabase;
using NexaCRM.UI.Services.Interfaces;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using NexaCRM.Service.Supabase;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseDealService : IDealService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseDealService> _logger;

    public SupabaseDealService(SupabaseClientProvider clientProvider, ILogger<SupabaseDealService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<IEnumerable<Deal>> GetDealsAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var dealsResponse = await client.From<DealRecord>()
                .Order(x => x.CreatedAt, PostgrestOrdering.Descending)
                .Get();

            if (dealsResponse.Models.Count == 0)
            {
                return Array.Empty<Deal>();
            }

            var stageIds = dealsResponse.Models.Select(d => d.StageId).Distinct().ToArray();
            var stageMap = await LoadStageLookupAsync(client, stageIds);

            return dealsResponse.Models.Select(record => MapToDeal(record, stageMap)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load deals from Supabase.");
            throw;
        }
    }

    public async Task<IReadOnlyList<DealStage>> GetDealStagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<DealStageRecord>()
                .Order(x => x.SortOrder, PostgrestOrdering.Ascending)
                .Get(cancellationToken: cancellationToken);

            if (response.Models.Count == 0)
            {
                return Array.Empty<DealStage>();
            }

            return response.Models
                .Select(stage => new DealStage
                {
                    Id = stage.Id,
                    Name = stage.Name,
                    SortOrder = stage.SortOrder
                })
                .OrderBy(stage => stage.SortOrder)
                .ThenBy(stage => stage.Name)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load deal stages from Supabase.");
            throw;
        }
    }

    public async Task<Deal> CreateDealAsync(DealCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var now = DateTime.UtcNow;
            var record = new DealRecord
            {
                Name = request.Name,
                StageId = request.StageId,
                Value = request.Amount,
                CompanyName = request.Company,
                ContactName = request.ContactName,
                AssignedToName = request.Owner,
                ExpectedCloseDate = request.ExpectedCloseDate,
                CreatedAt = now
            };

            var response = await client.From<DealRecord>()
                .Insert(record, cancellationToken: cancellationToken);

            var created = response.Models.FirstOrDefault() ?? record;

            var stageLookup = await LoadStageLookupAsync(client, new[] { created.StageId });
            return MapToDeal(created, stageLookup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create deal in Supabase.");
            throw;
        }
    }

    private static async Task<Dictionary<int, string>> LoadStageLookupAsync(Supabase.Client client, IEnumerable<int> stageIds)
    {
        var lookup = new Dictionary<int, string>();
        var ids = stageIds.Distinct().ToArray();

        if (ids.Length == 0)
        {
            return lookup;
        }

        var response = await client.From<DealStageRecord>()
            .Filter(x => x.Id, PostgrestOperator.In, ids)
            .Get();

        foreach (var stage in response.Models)
        {
            lookup[stage.Id] = stage.Name;
        }

        return lookup;
    }

    private static Deal MapToDeal(DealRecord record, IReadOnlyDictionary<int, string> stageMap)
    {
        var stageName = stageMap.TryGetValue(record.StageId, out var value) ? value : "Unknown";

        return new Deal
        {
            Id = record.Id,
            Name = record.Name,
            Stage = stageName,
            Amount = record.Value ?? 0,
            Company = record.CompanyName,
            ContactPerson = record.ContactName,
            Owner = record.AssignedToName ?? record.AssignedTo?.ToString(),
            CreatedDate = record.CreatedAt
        };
    }
}
