using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Organization;
using NexaCRM.Services.Admin.Models.Supabase;
using Supabase;
using AgentModel = NexaCRM.Services.Admin.Models.Agent;
using NewUserModel = NexaCRM.Services.Admin.Models.NewUser;

namespace NexaCRM.Services.Admin;

public sealed class OrganizationService : IOrganizationService
{
    private readonly Client _supabaseClient;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(Client supabaseClient, ILogger<OrganizationService> logger)
    {
        _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private readonly List<OrganizationUnit> _organizationUnits =
    [
        new(1, "Head Office", null),
        new(2, "Sales", 1),
        new(3, "Marketing", 1),
        new(4, "Customer Success", 2)
    ];

    private readonly List<OrganizationStats> _stats =
    [
        new("Head Office", 12),
        new("Sales", 8),
        new("Marketing", 6),
        new("Customer Success", 5)
    ];

    private readonly List<OrganizationUser> _users =
    [
        new()
        {
            Id = 1,
            UserId = "alice.kim-1",
            Name = "Alice Kim",
            Email = "alice.kim@example.com",
            Role = "Admin",
            Status = "Active",
            Department = "Operations",
            PhoneNumber = "010-1234-5678",
            RegisteredAt = DateTime.Today.AddMonths(-12),
            ApprovedAt = DateTime.Today.AddMonths(-12),
            ApprovalMemo = "최초 관리자 계정"
        },
        new()
        {
            Id = 2,
            UserId = "brian.lee-2",
            Name = "Brian Lee",
            Email = "brian.lee@example.com",
            Role = "Manager",
            Status = "Active",
            Department = "Sales",
            PhoneNumber = "010-3456-7890",
            RegisteredAt = DateTime.Today.AddMonths(-6),
            ApprovedAt = DateTime.Today.AddMonths(-6),
            ApprovalMemo = "영업 총괄 승인"
        },
        new()
        {
            Id = 3,
            UserId = "chloe.park-3",
            Name = "Chloe Park",
            Email = "chloe.park@example.com",
            Role = "Analyst",
            Status = "Inactive",
            Department = "Marketing",
            PhoneNumber = "010-9876-5432",
            RegisteredAt = DateTime.Today.AddMonths(-2),
            ApprovedAt = DateTime.Today.AddMonths(-2),
            ApprovalMemo = "휴직 처리"
        }
    ];

    private readonly List<AgentModel> _admins =
    [
        new() { Id = 1, Name = "Alice Kim", Email = "alice.kim@example.com", Role = "Admin" },
        new() { Id = 2, Name = "Brian Lee", Email = "brian.lee@example.com", Role = "Manager" }
    ];

    public Task<IEnumerable<OrganizationUnit>> GetOrganizationStructureAsync() =>
        Task.FromResult<IEnumerable<OrganizationUnit>>(_organizationUnits.ToList());

    public Task SaveOrganizationUnitAsync(OrganizationUnit unit)
    {
        if (unit.Id == 0)
        {
            var newUnit = unit with { Id = GenerateUnitId() };
            _organizationUnits.Add(newUnit);
        }
        else
        {
            var index = _organizationUnits.FindIndex(u => u.Id == unit.Id);
            if (index >= 0)
            {
                _organizationUnits[index] = unit;
            }
            else
            {
                _organizationUnits.Add(unit);
            }
        }

        return Task.CompletedTask;
    }

    public Task DeleteOrganizationUnitAsync(int id)
    {
        RemoveUnitRecursive(id);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<OrganizationStats>> GetOrganizationStatsAsync() =>
        Task.FromResult<IEnumerable<OrganizationStats>>(_stats.ToList());

    public Task<IEnumerable<AgentModel>> GetAdminsAsync() =>
        Task.FromResult<IEnumerable<AgentModel>>(_admins.Select(CloneAdmin).ToList());

    public Task AddAdminAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.CompletedTask;
        }

        if (_admins.Any(a => a.Id.ToString() == userId))
        {
            return Task.CompletedTask;
        }

        var id = int.TryParse(userId, out var parsed) ? parsed : GenerateAdminId();
        _admins.Add(new AgentModel
        {
            Id = id,
            Name = $"Admin {id}",
            Email = $"admin{id}@example.com",
            Role = "Admin"
        });

        return Task.CompletedTask;
    }

    public Task RemoveAdminAsync(string userId)
    {
        _admins.RemoveAll(a => a.Id.ToString() == userId);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<OrganizationUser>> GetUsersAsync() =>
        Task.FromResult<IEnumerable<OrganizationUser>>(_users.Select(CloneUser).ToList());

    public Task UpdateUserAsync(OrganizationUser user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing is not null)
        {
            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.Role = user.Role;
            existing.Status = user.Status;
            existing.Department = user.Department;
            existing.PhoneNumber = user.PhoneNumber;
            existing.RegisteredAt = user.RegisteredAt;
            existing.ApprovedAt = user.ApprovedAt;
            existing.ApprovalMemo = user.ApprovalMemo;
        }

        return Task.CompletedTask;
    }

    public Task DeleteUserAsync(int userId)
    {
        _users.RemoveAll(u => u.Id == userId);
        return Task.CompletedTask;
    }

    public Task ApproveUserAsync(int userId)
    {
        var existing = _users.FirstOrDefault(u => u.Id == userId);
        if (existing is not null)
        {
            existing.Status = "Active";
            existing.ApprovedAt = DateTime.Now;
            existing.ApprovalMemo = null;
        }

        return Task.CompletedTask;
    }

    public Task RejectUserAsync(int userId, string? reason)
    {
        var existing = _users.FirstOrDefault(u => u.Id == userId);
        if (existing is not null)
        {
            existing.Status = "Rejected";
            existing.ApprovedAt = null;
            existing.ApprovalMemo = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        }

        return Task.CompletedTask;
    }

    public Task SetSystemAdminAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.CompletedTask;
        }

        if (!int.TryParse(userId, out var parsedId))
        {
            return Task.CompletedTask;
        }

        var user = _users.FirstOrDefault(u => u.Id == parsedId);
        if (user is null)
        {
            return Task.CompletedTask;
        }

        user.Role = "Admin";
        user.Status = "Active";
        user.ApprovedAt ??= DateTime.UtcNow;
        user.ApprovalMemo = "Elevated to system administrator";

        if (_admins.All(a => a.Id != parsedId))
        {
            _admins.Add(new AgentModel
            {
                Id = parsedId,
                Name = user.Name,
                Email = user.Email,
                Role = "Admin"
            });
        }

        return Task.CompletedTask;
    }

    public async Task RegisterUserAsync(NewUserModel user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // 유효성 검사
        var results = new List<ValidationResult>();
        var context = new ValidationContext(user);

        if (!Validator.TryValidateObject(user, context, results, validateAllProperties: true))
        {
            var message = string.Join(" ", results
                .Select(result => result.ErrorMessage)
                .Where(error => !string.IsNullOrWhiteSpace(error)));

            if (string.IsNullOrWhiteSpace(message))
            {
                message = "Validation failed for user registration.";
            }

            throw new ValidationException(message);
        }

        try
        {
            _logger.LogInformation("사용자 등록 시도: {Email}", user.Email);

            // Supabase Auth를 통한 사용자 생성
            var signUpResponse = await _supabaseClient.Auth.SignUp(
                email: user.Email.Trim(),
                password: user.Password.Trim(),
                options: new Supabase.Gotrue.SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "full_name", user.FullName.Trim() },
                        { "user_id", user.UserId.Trim() },
                        { "role", "Member" },
                        { "status", "Pending" }
                    }
                }
            );

            if (signUpResponse?.User == null)
            {
                throw new InvalidOperationException("사용자 생성에 실패했습니다.");
            }

            _logger.LogInformation("사용자 등록 성공: {Email}, Supabase User ID: {UserId}", 
                user.Email, signUpResponse.User.Id);

            // 사용자 데이터를 app_users와 user_infos 테이블에 저장
            try
            {
                var authUserId = string.IsNullOrEmpty(signUpResponse.User.Id) ? (Guid?)null : Guid.Parse(signUpResponse.User.Id);
                var cuid = Guid.NewGuid().ToString("N"); // CUID 생성

                // 1. app_users 테이블에 먼저 삽입
                var appUserRecord = new AppUserRecord
                {
                    Cuid = cuid,
                    AuthUserId = authUserId,
                    Email = user.Email.Trim(),
                    Status = "Pending"
                };

                await _supabaseClient
                    .From<AppUserRecord>()
                    .Insert(appUserRecord);

                _logger.LogInformation("app_users 테이블에 사용자 추가 완료: {Email}, CUID: {Cuid}", user.Email, cuid);

                // 2. user_infos 테이블에 상세 정보 삽입
                var userInfoRecord = new UserInfoRecord
                {
                    UserCuid = cuid,
                    Username = user.UserId.Trim(),
                    FullName = user.FullName.Trim(),
                    Department = null,
                    PhoneNumber = null,
                    Role = "Member",
                    Status = "Pending",
                    RegisteredAt = DateTime.UtcNow
                };

                await _supabaseClient
                    .From<UserInfoRecord>()
                    .Insert(userInfoRecord);

                _logger.LogInformation("user_infos 테이블에 사용자 정보 추가 완료: {Username}, CUID: {Cuid}", user.UserId, cuid);

                // 3. profiles 테이블에 공개 프로필 삽입
                if (authUserId.HasValue)
                {
                    var profileRecord = new ProfileRecord
                    {
                        Id = authUserId.Value,
                        UserCuid = cuid,
                        Username = user.UserId.Trim(),
                        FullName = user.FullName.Trim(),
                        AvatarUrl = null
                    };

                    await _supabaseClient
                        .From<ProfileRecord>()
                        .Insert(profileRecord);

                    _logger.LogInformation("profiles 테이블에 공개 프로필 추가 완료: {Username}", user.UserId);
                }

                // 4. organization_users 테이블에 조직 멤버십 삽입
                if (authUserId.HasValue)
                {
                    var orgUserRecord = new OrganizationUserRecord
                    {
                        UserId = authUserId.Value,
                        UserCuid = cuid,
                        UnitId = null, // 기본 조직 단위 없음 (나중에 관리자가 할당)
                        Role = "Member",
                        Status = "pending",
                        Department = null,
                        PhoneNumber = null,
                        RegisteredAt = DateTime.UtcNow,
                        ApprovedAt = null,
                        ApprovalMemo = "신규 회원가입"
                    };

                    await _supabaseClient
                        .From<OrganizationUserRecord>()
                        .Insert(orgUserRecord);

                    _logger.LogInformation("organization_users 테이블에 조직 멤버십 추가 완료: {Username}", user.UserId);
                }

                // 메모리 리스트에도 추가 (임시 - 나중에 DB에서 조회로 변경 가능)
                var newUser = new OrganizationUser
                {
                    Id = GenerateUserId(),
                    UserId = user.UserId.Trim(),
                    Name = user.FullName.Trim(),
                    Email = user.Email.Trim(),
                    Role = "Member",
                    Status = "Pending",
                    Department = string.Empty,
                    PhoneNumber = string.Empty,
                    RegisteredAt = DateTime.UtcNow,
                    ApprovedAt = null,
                    ApprovalMemo = "Registration requested via Supabase Auth"
                };

                _users.Add(newUser);

                _logger.LogInformation("모든 테이블에 사용자 데이터 삽입 완료: {Email}", user.Email);
            }
            catch (Exception dbEx)
            {
                _logger.LogWarning(dbEx, "데이터베이스 테이블 삽입 실패 (Auth 가입은 성공): {Email}", user.Email);
                // 데이터베이스 삽입 실패해도 Auth 가입은 성공했으므로 계속 진행
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "사용자 등록 실패: {Email}", user.Email);
            
            // 사용자 친화적인 에러 메시지
            if (ex.Message.Contains("already registered") || ex.Message.Contains("User already registered"))
            {
                throw new InvalidOperationException("이미 등록된 이메일입니다.");
            }
            
            throw new InvalidOperationException($"사용자 등록 중 오류가 발생했습니다: {ex.Message}", ex);
        }
    }

    public Task<IEnumerable<NewUserModel>> GetPendingUsersAsync()
    {
        var pending = _users
            .Where(u => string.Equals(u.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            .Select(u => new NewUserModel
            {
                UserId = u.UserId ?? string.Empty,
                FullName = u.Name ?? string.Empty,
                Email = u.Email,
                Password = string.Empty,
                ConfirmPassword = string.Empty,
                TermsAccepted = true
            })
            .ToList();

        return Task.FromResult<IEnumerable<NewUserModel>>(pending);
    }

    public Task InviteUserAsync(NewUserModel newUser)
    {
        if (string.IsNullOrWhiteSpace(newUser.Email))
        {
            return Task.CompletedTask;
        }

        var id = GenerateUserId();
        var name = string.IsNullOrWhiteSpace(newUser.FullName)
            ? $"Invited {id}"
            : newUser.FullName.Trim();
        var userId = string.IsNullOrWhiteSpace(newUser.UserId)
            ? $"invited.{id}-user"
            : newUser.UserId.Trim();
        _users.Add(new OrganizationUser
        {
            Id = id,
            UserId = userId,
            Name = name,
            Email = newUser.Email.Trim(),
            Role = "Member",
            Status = "Pending",
            Department = string.Empty,
            RegisteredAt = DateTime.UtcNow,
            ApprovalMemo = "Invitation sent"
        });

        return Task.CompletedTask;
    }

    private static AgentModel CloneAdmin(AgentModel admin) => new()
    {
        Id = admin.Id,
        Name = admin.Name,
        Email = admin.Email,
        Role = admin.Role
    };

    private static OrganizationUser CloneUser(OrganizationUser user) => new()
    {
        Id = user.Id,
        UserId = user.UserId,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        Status = user.Status,
        Department = user.Department,
        PhoneNumber = user.PhoneNumber,
        RegisteredAt = user.RegisteredAt,
        ApprovedAt = user.ApprovedAt,
        ApprovalMemo = user.ApprovalMemo
    };

    private int GenerateUnitId() => _organizationUnits.Count == 0 ? 1 : _organizationUnits.Max(u => u.Id) + 1;

    private int GenerateAdminId() => _admins.Count == 0 ? 1 : _admins.Max(a => a.Id) + 1;

    private int GenerateUserId() => _users.Count == 0 ? 1 : _users.Max(u => u.Id) + 1;

    private void RemoveUnitRecursive(int id)
    {
        var toRemove = _organizationUnits.Where(u => u.ParentId == id).Select(u => u.Id).ToList();
        foreach (var childId in toRemove)
        {
            RemoveUnitRecursive(childId);
        }

        _organizationUnits.RemoveAll(u => u.Id == id);
    }
}
