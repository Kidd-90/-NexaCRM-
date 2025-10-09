using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.UI.Models.Teams;

namespace NexaCRM.UI.Services.Interfaces;

public interface ITeamService
{
    Task<IReadOnlyList<TeamInfo>> GetTeamsAsync();
    Task<IReadOnlyList<TeamMemberInfo>> GetTeamMembersAsync();
    Task<TeamInfo> CreateTeamAsync(TeamInfo team);
    Task<TeamMemberInfo> CreateTeamMemberAsync(TeamMemberInfo member);
    Task UpdateTeamStatusAsync(long teamId, bool isActive);
    Task UpdateTeamMemberStatusAsync(long memberId, bool isActive);
    Task UpdateTeamMemberUploadPermissionAsync(long memberId, bool allow);
}
