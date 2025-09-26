using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Teams;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface ITeamService
{
    Task<IReadOnlyList<TeamInfo>> GetTeamsAsync();
    Task<IReadOnlyList<TeamMemberInfo>> GetTeamMembersAsync();
    Task<TeamInfo> CreateTeamAsync(TeamInfo team);
    Task<TeamMemberInfo> CreateTeamMemberAsync(TeamMemberInfo member);
    Task UpdateTeamStatusAsync(int teamId, bool isActive);
    Task UpdateTeamMemberStatusAsync(int memberId, bool isActive);
    Task UpdateTeamMemberUploadPermissionAsync(int memberId, bool allow);
}
