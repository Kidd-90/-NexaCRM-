using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.UI.Models;
using NexaCRM.UI.Models.Supabase;
using NexaCRM.UI.Services.Interfaces;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using AgentModel = NexaCRM.Services.Admin.Models.Agent;

namespace NexaCRM.Service.Supabase;

public sealed class SupabaseAgentService : IAgentService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseAgentService> _logger;

    public SupabaseAgentService(SupabaseClientProvider clientProvider, ILogger<SupabaseAgentService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<IEnumerable<AgentModel>> GetAgentsAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<AgentRecord>()
                .Order(x => x.DisplayName, PostgrestOrdering.Ascending)
                .Get();

            var records = response.Models ?? new List<AgentRecord>();
            if (records.Count == 0)
            {
                return Array.Empty<AgentModel>();
            }

            return records
                .Where(record => record.IsActive)
                .Select(MapToAgent)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load agents from Supabase.");
            throw;
        }
    }

    private static AgentModel MapToAgent(AgentRecord record)
    {
        return new AgentModel
        {
            Id = record.Id,
            Name = record.DisplayName,
            Email = record.Email,
            Role = record.Role
        };
    }
}
