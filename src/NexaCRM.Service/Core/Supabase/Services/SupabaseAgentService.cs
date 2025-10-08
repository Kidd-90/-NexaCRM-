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
            _logger.LogInformation("Loading agents from Supabase...");
            var client = await _clientProvider.GetClientAsync();
            _logger.LogInformation("Supabase client obtained successfully");
            
            _logger.LogInformation("Executing query on agents table...");
            var response = await client.From<AgentRecord>()
                .Order(x => x.DisplayName, PostgrestOrdering.Ascending)
                .Get();

            _logger.LogInformation("Query executed. Response received");
            _logger.LogInformation("Response Model Count: {Count}", response.Models?.Count ?? 0);
            _logger.LogInformation("Response Content: {Content}", response.Content ?? "null");

            var records = response.Models ?? new List<AgentRecord>();
            _logger.LogInformation("Loaded {Count} agent records from Supabase", records.Count);
            
            if (records.Count == 0)
            {
                _logger.LogWarning("No agent records found in database. Check if data exists and RLS policies are correct.");
                return Array.Empty<AgentModel>();
            }

            var activeRecords = records.Where(record => record.IsActive).ToList();
            _logger.LogInformation("Found {Count} active agents (filtered from {Total} total)", activeRecords.Count, records.Count);
            
            if (activeRecords.Count > 0)
            {
                _logger.LogInformation("First active agent sample - Id: {Id}, Name: {Name}, Role: {Role}", 
                    activeRecords[0].Id, activeRecords[0].DisplayName, activeRecords[0].Role);
            }

            return activeRecords.Select(MapToAgent).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load agents from Supabase. Error: {Message}", ex.Message);
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
