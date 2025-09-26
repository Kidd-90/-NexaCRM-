using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.UI.Models.Teams;
using NexaCRM.UI.Models.Supabase;
using NexaCRM.UI.Services.Interfaces;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseTeamService : ITeamService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseTeamService> _logger;

    public SupabaseTeamService(SupabaseClientProvider clientProvider, ILogger<SupabaseTeamService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TeamInfo>> GetTeamsAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<TeamRecord>()
                .Order(x => x.Name, PostgrestOrdering.Ascending)
                .Get();

            var records = response.Models ?? new List<TeamRecord>();
            return records.Select(MapToTeam).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load teams from Supabase.");
            throw;
        }
    }

    public async Task<IReadOnlyList<TeamMemberInfo>> GetTeamMembersAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<TeamMemberRecord>()
                .Order(x => x.RegisteredAt, PostgrestOrdering.Descending)
                .Get();

            var records = response.Models ?? new List<TeamMemberRecord>();
            return records.Select(MapToTeamMember).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load team members from Supabase.");
            throw;
        }
    }

    public async Task<TeamInfo> CreateTeamAsync(TeamInfo team)
    {
        ArgumentNullException.ThrowIfNull(team);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = new TeamRecord
            {
                Code = team.TeamCode,
                Name = team.Name,
                ManagerName = team.ManagerName,
                MemberCount = team.MemberCount,
                IsActive = team.IsActive,
                RegisteredAt = team.RegisteredAt == default ? DateTime.UtcNow : team.RegisteredAt,
                UpdatedAt = DateTime.UtcNow
            };

            var response = await client.From<TeamRecord>()
                .Insert(record);

            return MapToTeam(response.Models.FirstOrDefault() ?? record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create team {TeamName} in Supabase.", team.Name);
            throw;
        }
    }

    public async Task<TeamMemberInfo> CreateTeamMemberAsync(TeamMemberInfo member)
    {
        ArgumentNullException.ThrowIfNull(member);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = new TeamMemberRecord
            {
                TeamId = member.TeamId,
                TeamName = member.TeamName,
                Role = member.Role,
                EmployeeCode = member.EmployeeCode,
                Username = member.Username,
                FullName = member.FullName,
                AllowExcelUpload = member.AllowExcelUpload,
                IsActive = member.IsActive,
                RegisteredAt = member.RegisteredAt == default ? DateTime.UtcNow : member.RegisteredAt,
                UpdatedAt = DateTime.UtcNow
            };

            var response = await client.From<TeamMemberRecord>()
                .Insert(record);

            return MapToTeamMember(response.Models.FirstOrDefault() ?? record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create team member {Username} in Supabase.", member.Username);
            throw;
        }
    }

    public async Task UpdateTeamStatusAsync(int teamId, bool isActive)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<TeamRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, teamId)
                .Get();

            var record = response.Models.FirstOrDefault();
            if (record is null)
            {
                return;
            }

            record.IsActive = isActive;
            record.UpdatedAt = DateTime.UtcNow;

            await client.From<TeamRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, teamId)
                .Update(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update team {TeamId} status in Supabase.", teamId);
            throw;
        }
    }

    public async Task UpdateTeamMemberStatusAsync(int memberId, bool isActive)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<TeamMemberRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, memberId)
                .Get();

            var record = response.Models.FirstOrDefault();
            if (record is null)
            {
                return;
            }

            record.IsActive = isActive;
            record.UpdatedAt = DateTime.UtcNow;

            await client.From<TeamMemberRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, memberId)
                .Update(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update team member {MemberId} status in Supabase.", memberId);
            throw;
        }
    }

    public async Task UpdateTeamMemberUploadPermissionAsync(int memberId, bool allow)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<TeamMemberRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, memberId)
                .Get();

            var record = response.Models.FirstOrDefault();
            if (record is null)
            {
                return;
            }

            record.AllowExcelUpload = allow;
            record.UpdatedAt = DateTime.UtcNow;

            await client.From<TeamMemberRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, memberId)
                .Update(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update team member {MemberId} upload permission in Supabase.", memberId);
            throw;
        }
    }

    private static TeamInfo MapToTeam(TeamRecord record)
    {
        return new TeamInfo
        {
            Id = record.Id,
            TeamCode = record.Code,
            Name = record.Name,
            ManagerName = record.ManagerName ?? string.Empty,
            MemberCount = record.MemberCount,
            IsActive = record.IsActive,
            RegisteredAt = record.RegisteredAt
        };
    }

    private static TeamMemberInfo MapToTeamMember(TeamMemberRecord record)
    {
        return new TeamMemberInfo
        {
            Id = record.Id,
            TeamId = record.TeamId,
            TeamName = record.TeamName ?? string.Empty,
            Role = record.Role,
            EmployeeCode = record.EmployeeCode,
            Username = record.Username,
            FullName = record.FullName,
            AllowExcelUpload = record.AllowExcelUpload,
            IsActive = record.IsActive,
            RegisteredAt = record.RegisteredAt
        };
    }
}
