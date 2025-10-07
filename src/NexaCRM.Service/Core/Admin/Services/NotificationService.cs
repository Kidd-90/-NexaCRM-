using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using NexaCRM.Service.Supabase;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Settings;
using NexaCRM.UI.Models.Supabase;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;

namespace NexaCRM.Services.Admin;

public sealed class NotificationService : INotificationService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<NotificationService> _logger;
    private Guid? _userId;

    public NotificationService(
        SupabaseClientProvider clientProvider,
        AuthenticationStateProvider authStateProvider,
        ILogger<NotificationService> logger)
    {
        _clientProvider = clientProvider;
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    public async Task<NotificationSettings> GetSettingsAsync()
    {
        try
        {
            var (hasUserId, userId) = await TryEnsureUserIdAsync();
            if (!hasUserId)
            {
                _logger.LogWarning("[GetSettingsAsync] No authenticated user found, returning default settings");
                return CreateDefaultSettings();
            }

            _logger.LogInformation("[GetSettingsAsync] Loading settings for user: {UserId}", userId);

            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<NotificationSettingsRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId.ToString())
                .Get();

            var record = response.Models?.FirstOrDefault();

            if (record == null)
            {
                _logger.LogInformation("[GetSettingsAsync] No settings found for user {UserId}, creating defaults", userId);
                return await CreateAndSaveDefaultSettingsAsync(userId);
            }

            _logger.LogInformation("[GetSettingsAsync] Settings loaded successfully for user {UserId}", userId);
            return MapToSettings(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetSettingsAsync] Failed to load notification settings");
            return CreateDefaultSettings();
        }
    }

    public async Task SaveSettingsAsync(NotificationSettings settings)
    {
        try
        {
            var (hasUserId, userId) = await TryEnsureUserIdAsync();
            
            _logger.LogInformation("[SaveSettingsAsync] 🔍 TryEnsureUserIdAsync returned: {HasUserId}, {UserId}", hasUserId, userId);
            
            if (!hasUserId)
            {
                _logger.LogError("[SaveSettingsAsync] ❌ No authenticated user found, cannot save settings");
                throw new InvalidOperationException("User must be authenticated to save notification settings");
            }

            _logger.LogInformation("[SaveSettingsAsync] ✅ Saving settings for user: {UserId}", userId);

            var client = await _clientProvider.GetClientAsync();
            var record = MapToRecord(settings, userId);
            
            _logger.LogInformation("[SaveSettingsAsync] 📝 Record created with UserId: {RecordUserId}", record.UserId);

            // Check if record exists
            var existingResponse = await client.From<NotificationSettingsRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId.ToString())
                .Get();

            if (existingResponse.Models?.Any() == true)
            {
                // Update existing record
                await client.From<NotificationSettingsRecord>()
                    .Filter(x => x.UserId, PostgrestOperator.Equals, userId.ToString())
                    .Update(record);
                _logger.LogInformation("[SaveSettingsAsync] Settings updated for user {UserId}", userId);
            }
            else
            {
                // Insert new record
                await client.From<NotificationSettingsRecord>()
                    .Insert(record);
                _logger.LogInformation("[SaveSettingsAsync] Settings created for user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SaveSettingsAsync] Failed to save notification settings");
            throw;
        }
    }

    private async Task<NotificationSettings> CreateAndSaveDefaultSettingsAsync(Guid userId)
    {
        try
        {
            var defaultSettings = CreateDefaultSettings();
            var client = await _clientProvider.GetClientAsync();
            var record = MapToRecord(defaultSettings, userId);

            await client.From<NotificationSettingsRecord>()
                .Insert(record);

            _logger.LogInformation("[CreateAndSaveDefaultSettingsAsync] Default settings created for user {UserId}", userId);
            return defaultSettings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CreateAndSaveDefaultSettingsAsync] Failed to create default settings");
            return CreateDefaultSettings();
        }
    }

    private async Task<(bool success, Guid userId)> TryEnsureUserIdAsync()
    {
        try
        {
            // Try to get user ID from AuthenticationStateProvider
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            
            Console.WriteLine($"[TryEnsureUserIdAsync] 🔍 User authenticated: {user?.Identity?.IsAuthenticated}");
            _logger.LogInformation("[TryEnsureUserIdAsync] 🔍 User authenticated: {IsAuth}, Name: {Name}", 
                user?.Identity?.IsAuthenticated, user?.Identity?.Name);
            
            if (user?.Identity?.IsAuthenticated == true)
            {
                // 모든 Claims 로깅
                Console.WriteLine($"[TryEnsureUserIdAsync] 📋 Total claims: {user.Claims.Count()}");
                foreach (var claim in user.Claims)
                {
                    Console.WriteLine($"[TryEnsureUserIdAsync] Claim: {claim.Type} = {claim.Value}");
                    _logger.LogInformation("[TryEnsureUserIdAsync] Claim: {Type} = {Value}", claim.Type, claim.Value);
                }

                // Try to get user ID from NameIdentifier claim
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                Console.WriteLine($"[TryEnsureUserIdAsync] NameIdentifier claim: {userIdClaim?.Value ?? "NULL"}");
                
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsed))
                {
                    Console.WriteLine($"[TryEnsureUserIdAsync] ✅ User ID from claims: {parsed}");
                    _logger.LogInformation("[TryEnsureUserIdAsync] ✅ User ID from claims: {UserId}", parsed);
                    
                    if (!_userId.HasValue || _userId.Value != parsed)
                    {
                        _userId = parsed;
                    }
                    
                    return (true, _userId.Value);
                }
                else
                {
                    Console.WriteLine("[TryEnsureUserIdAsync] ❌ No NameIdentifier claim found or failed to parse");
                    _logger.LogWarning("[TryEnsureUserIdAsync] ❌ No NameIdentifier claim found or failed to parse");
                }
            }
            else
            {
                Console.WriteLine("[TryEnsureUserIdAsync] ❌ User not authenticated");
                _logger.LogWarning("[TryEnsureUserIdAsync] ❌ User not authenticated");
            }
            
            // Fallback: Try to get from Supabase Client
            try
            {
                Console.WriteLine("[TryEnsureUserIdAsync] 🔄 Trying fallback: Supabase Client Auth");
                var client = await _clientProvider.GetClientAsync();
                var currentUser = client?.Auth?.CurrentUser;
                if (currentUser != null && !string.IsNullOrEmpty(currentUser.Id))
                {
                    Console.WriteLine($"[TryEnsureUserIdAsync] 🔄 Supabase CurrentUser.Id: {currentUser.Id}");
                    if (Guid.TryParse(currentUser.Id, out var fallbackUserId))
                    {
                        Console.WriteLine($"[TryEnsureUserIdAsync] ✅ User ID from Supabase Client: {fallbackUserId}");
                        _logger.LogInformation("[TryEnsureUserIdAsync] ✅ User ID from Supabase Client (fallback): {UserId}", fallbackUserId);
                        _userId = fallbackUserId;
                        return (true, fallbackUserId);
                    }
                }
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"[TryEnsureUserIdAsync] ❌ Fallback failed: {fallbackEx.Message}");
                _logger.LogError(fallbackEx, "[TryEnsureUserIdAsync] Fallback to Supabase Client failed");
            }
            
            return (false, Guid.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TryEnsureUserIdAsync] ❌ Exception: {ex.Message}");
            _logger.LogError(ex, "[TryEnsureUserIdAsync] ❌ Error getting user ID from authentication state");
            return (false, Guid.Empty);
        }
    }

    private static NotificationSettings CreateDefaultSettings()
    {
        return new NotificationSettings
        {
            NewLeadCreated = true,
            LeadStatusUpdated = true,
            DealStageChanged = true,
            DealValueUpdated = true,
            NewTaskAssigned = true,
            TaskDueDateReminder = true,
            EmailNotifications = true,
            InAppNotifications = true,
            PushNotifications = false
        };
    }

    private static NotificationSettings MapToSettings(NotificationSettingsRecord record)
    {
        return new NotificationSettings
        {
            NewLeadCreated = record.NewLeadCreated,
            LeadStatusUpdated = record.LeadStatusUpdated,
            DealStageChanged = record.DealStageChanged,
            DealValueUpdated = record.DealValueUpdated,
            NewTaskAssigned = record.NewTaskAssigned,
            TaskDueDateReminder = record.TaskDueDateReminder,
            EmailNotifications = record.EmailNotifications,
            InAppNotifications = record.InAppNotifications,
            PushNotifications = record.PushNotifications
        };
    }

    private static NotificationSettingsRecord MapToRecord(NotificationSettings settings, Guid userId)
    {
        return new NotificationSettingsRecord
        {
            UserId = userId,
            NewLeadCreated = settings.NewLeadCreated,
            LeadStatusUpdated = settings.LeadStatusUpdated,
            DealStageChanged = settings.DealStageChanged,
            DealValueUpdated = settings.DealValueUpdated,
            NewTaskAssigned = settings.NewTaskAssigned,
            TaskDueDateReminder = settings.TaskDueDateReminder,
            EmailNotifications = settings.EmailNotifications,
            InAppNotifications = settings.InAppNotifications,
            PushNotifications = settings.PushNotifications,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
