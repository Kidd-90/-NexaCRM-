using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Teams;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services.Mock;

public class MockTeamService : ITeamService
{
    private readonly List<TeamInfo> _teams = new()
    {
        new TeamInfo
        {
            Id = 1,
            TeamCode = "TM0001",
            Name = "SKY지점",
            ManagerName = "김태영",
            MemberCount = 8,
            IsActive = true,
            RegisteredAt = DateTime.Today.AddDays(-15)
        },
        new TeamInfo
        {
            Id = 2,
            TeamCode = "TM0002",
            Name = "프라임지점",
            ManagerName = "이하늘",
            MemberCount = 5,
            IsActive = true,
            RegisteredAt = DateTime.Today.AddDays(-30)
        },
        new TeamInfo
        {
            Id = 3,
            TeamCode = "TM0003",
            Name = "직영",
            ManagerName = "박지원",
            MemberCount = 3,
            IsActive = false,
            RegisteredAt = DateTime.Today.AddMonths(-2)
        }
    };

    private readonly List<TeamMemberInfo> _members = new()
    {
        new TeamMemberInfo
        {
            Id = 1,
            TeamId = 1,
            TeamName = "SKY지점",
            Role = "직책",
            EmployeeCode = "FM0017",
            Username = "fineeee",
            FullName = "이현정",
            AllowExcelUpload = true,
            IsActive = true,
            RegisteredAt = DateTime.Today.AddDays(-5)
        },
        new TeamMemberInfo
        {
            Id = 2,
            TeamId = 2,
            TeamName = "프라임지점",
            Role = "직책",
            EmployeeCode = "FC0005",
            Username = "abc2",
            FullName = "이상현",
            AllowExcelUpload = true,
            IsActive = true,
            RegisteredAt = DateTime.Today.AddDays(-12)
        },
        new TeamMemberInfo
        {
            Id = 3,
            TeamId = 3,
            TeamName = "직영",
            Role = "직책",
            EmployeeCode = "FM0001",
            Username = "abcd",
            FullName = "차은우",
            AllowExcelUpload = false,
            IsActive = false,
            RegisteredAt = DateTime.Today.AddMonths(-1)
        }
    };

    public Task<IReadOnlyList<TeamInfo>> GetTeamsAsync() =>
        Task.FromResult<IReadOnlyList<TeamInfo>>(_teams.Select(CloneTeam).ToList());

    public Task<IReadOnlyList<TeamMemberInfo>> GetTeamMembersAsync() =>
        Task.FromResult<IReadOnlyList<TeamMemberInfo>>(_members.Select(CloneMember).ToList());

    public Task<TeamInfo> CreateTeamAsync(TeamInfo team)
    {
        ArgumentNullException.ThrowIfNull(team);

        var newTeam = new TeamInfo
        {
            Id = GenerateTeamId(),
            TeamCode = string.IsNullOrWhiteSpace(team.TeamCode) ? GenerateTeamCode() : team.TeamCode,
            Name = team.Name,
            ManagerName = team.ManagerName,
            MemberCount = team.MemberCount,
            IsActive = team.IsActive,
            RegisteredAt = team.RegisteredAt == default ? DateTime.Today : team.RegisteredAt
        };

        _teams.Add(newTeam);
        return Task.FromResult(CloneTeam(newTeam));
    }

    public Task<TeamMemberInfo> CreateTeamMemberAsync(TeamMemberInfo member)
    {
        ArgumentNullException.ThrowIfNull(member);

        var team = _teams.FirstOrDefault(t => t.Id == member.TeamId) ??
                   _teams.FirstOrDefault(t => string.Equals(t.Name, member.TeamName, StringComparison.OrdinalIgnoreCase));

        if (team is null)
        {
            throw new InvalidOperationException("Team not found for the provided member.");
        }

        var newMember = new TeamMemberInfo
        {
            Id = GenerateMemberId(),
            TeamId = team.Id,
            TeamName = team.Name,
            Role = member.Role,
            EmployeeCode = string.IsNullOrWhiteSpace(member.EmployeeCode) ? GenerateMemberCode() : member.EmployeeCode,
            Username = member.Username,
            FullName = member.FullName,
            AllowExcelUpload = member.AllowExcelUpload,
            IsActive = member.IsActive,
            RegisteredAt = member.RegisteredAt == default ? DateTime.Today : member.RegisteredAt
        };

        _members.Add(newMember);
        return Task.FromResult(CloneMember(newMember));
    }

    public Task UpdateTeamStatusAsync(int teamId, bool isActive)
    {
        var team = _teams.FirstOrDefault(t => t.Id == teamId);
        if (team is not null)
        {
            team.IsActive = isActive;
        }

        return Task.CompletedTask;
    }

    public Task UpdateTeamMemberStatusAsync(int memberId, bool isActive)
    {
        var member = _members.FirstOrDefault(m => m.Id == memberId);
        if (member is not null)
        {
            member.IsActive = isActive;
        }

        return Task.CompletedTask;
    }

    public Task UpdateTeamMemberUploadPermissionAsync(int memberId, bool allow)
    {
        var member = _members.FirstOrDefault(m => m.Id == memberId);
        if (member is not null)
        {
            member.AllowExcelUpload = allow;
        }

        return Task.CompletedTask;
    }

    private static TeamInfo CloneTeam(TeamInfo source) => new()
    {
        Id = source.Id,
        TeamCode = source.TeamCode,
        Name = source.Name,
        ManagerName = source.ManagerName,
        MemberCount = source.MemberCount,
        IsActive = source.IsActive,
        RegisteredAt = source.RegisteredAt
    };

    private static TeamMemberInfo CloneMember(TeamMemberInfo source) => new()
    {
        Id = source.Id,
        TeamId = source.TeamId,
        TeamName = source.TeamName,
        Role = source.Role,
        EmployeeCode = source.EmployeeCode,
        Username = source.Username,
        FullName = source.FullName,
        AllowExcelUpload = source.AllowExcelUpload,
        IsActive = source.IsActive,
        RegisteredAt = source.RegisteredAt
    };

    private int GenerateTeamId() => _teams.Count == 0 ? 1 : _teams.Max(t => t.Id) + 1;

    private string GenerateTeamCode()
    {
        var next = _teams.Count == 0 ? 1 : _teams.Max(t => ParseSuffix(t.TeamCode)) + 1;
        return $"TM{next:0000}";
    }

    private int GenerateMemberId() => _members.Count == 0 ? 1 : _members.Max(m => m.Id) + 1;

    private string GenerateMemberCode()
    {
        var next = _members.Count == 0 ? 1 : _members.Max(m => ParseSuffix(m.EmployeeCode)) + 1;
        return $"FM{next:0000}";
    }

    private static int ParseSuffix(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length <= 2)
        {
            return 0;
        }

        return int.TryParse(code.AsSpan(2), out var value) ? value : 0;
    }
}
