# Service Interfaces Reference Guide

## Overview

This reference guide provides comprehensive documentation for all service interfaces in the NexaCRM Web Client application. Each service interface defines a contract for specific business functionality, enabling clean separation of concerns and testability through dependency injection.

**Interface Design Principles:**
- **Single Responsibility**: Each interface focuses on one specific business domain
- **Async Operations**: All operations use async/await patterns for scalability
- **Consistent Naming**: Standardized naming conventions across all interfaces
- **Error Handling**: Proper exception handling and error propagation
- **Testability**: Interface-based design enables easy mocking and unit testing

## Currently Implemented Service Interfaces

### IContactService - Contact Management Interface

**Purpose**: Manages customer contact information and related operations

**Namespace**: `NexaCRM.WebClient.Services.Interfaces`

#### Interface Definition
```csharp
public interface IContactService
{
    Task<IEnumerable<Contact>> GetContactsAsync();
}
```

#### Method Details

**GetContactsAsync()**
- **Purpose**: Retrieves all contacts accessible to the current user
- **Return Type**: `Task<IEnumerable<Contact>>`
- **Parameters**: None
- **Exceptions**: 
  - `UnauthorizedAccessException`: User doesn't have permission to view contacts
  - `ServiceException`: General service error occurred

#### Usage Example
```csharp
@inject IContactService ContactService

private async Task LoadContacts()
{
    try
    {
        var contacts = await ContactService.GetContactsAsync();
        // Process contacts
    }
    catch (UnauthorizedAccessException)
    {
        // Handle authorization error
    }
    catch (Exception ex)
    {
        // Handle general error
    }
}
```

#### Implementation Notes
- **Current Implementation**: MockContactService provides sample data
- **Future Enhancement**: Will support filtering, paging, and search parameters
- **Performance**: Consider implementing caching for frequently accessed contact lists
- **Security**: Row-level security will be enforced based on user permissions

---

### IDealService - Deal/Opportunity Management Interface

**Purpose**: Handles sales opportunities, deal pipeline management, and revenue tracking

**Namespace**: `NexaCRM.WebClient.Services.Interfaces`

#### Interface Definition
```csharp
public interface IDealService
{
    Task<IEnumerable<Deal>> GetDealsAsync();
}
```

#### Method Details

**GetDealsAsync()**
- **Purpose**: Retrieves all deals/opportunities accessible to the current user
- **Return Type**: `Task<IEnumerable<Deal>>`
- **Parameters**: None
- **Exceptions**:
  - `UnauthorizedAccessException`: User doesn't have permission to view deals
  - `ServiceException`: General service error occurred

#### Usage Example
```csharp
@inject IDealService DealService

private async Task LoadDeals()
{
    try
    {
        var deals = await DealService.GetDealsAsync();
        var pipelineValue = deals.Sum(d => d.Value);
    }
    catch (Exception ex)
    {
        // Handle error
    }
}
```

#### Implementation Notes
- **Current Implementation**: MockDealService provides sample deal data
- **Future Enhancement**: Will include filtering by stage, assigned user, date range
- **Pipeline Integration**: Designed to support Kanban-style pipeline visualization
- **Reporting**: Deal data feeds into sales performance reports

---

### ITaskService - Task Management Interface with Full CRUD

**Purpose**: Comprehensive task lifecycle management with full create, read, update, delete operations

**Namespace**: `NexaCRM.WebClient.Services.Interfaces`

#### Interface Definition
```csharp
public interface ITaskService
{
    Task<IEnumerable<Models.Task>> GetTasksAsync();
    Task<Models.Task> GetTaskByIdAsync(int id);
    Task CreateTaskAsync(Models.Task task);
    Task UpdateTaskAsync(Models.Task task);
    Task DeleteTaskAsync(int id);
}
```

#### Method Details

**GetTasksAsync()**
- **Purpose**: Retrieves all tasks accessible to the current user
- **Return Type**: `Task<IEnumerable<Models.Task>>`
- **Parameters**: None
- **Exceptions**: `ServiceException`, `UnauthorizedAccessException`

**GetTaskByIdAsync(int id)**
- **Purpose**: Retrieves a specific task by its unique identifier
- **Return Type**: `Task<Models.Task>`
- **Parameters**: 
  - `id` (int): Unique task identifier
- **Returns**: Task object or null if not found
- **Exceptions**: `TaskNotFoundException`, `UnauthorizedAccessException`

**CreateTaskAsync(Models.Task task)**
- **Purpose**: Creates a new task in the system
- **Return Type**: `Task` (void)
- **Parameters**:
  - `task` (Models.Task): Task object with required properties
- **Validation**: Title and assigned user are required
- **Exceptions**: `ValidationException`, `ServiceException`

---

### IUserGovernanceService - Account Lifecycle and Security Governance

**Purpose**: Manages the full lifecycle of authenticated users including provisioning, role assignment, password resets, and security policy enforcement.

**Namespace**: `NexaCRM.WebClient.Services.Interfaces`

#### Key Methods
- `CreateUserAsync(string email, string displayName, IEnumerable<string> roles, CancellationToken ct)`
  - Creates a new Supabase-backed identity and assigns initial roles.
- `AssignRolesAsync(Guid userId, IEnumerable<string> roles, CancellationToken ct)`
  - Persists role membership ensuring audit logging is written.
- `CreatePasswordResetTicketAsync(Guid userId, CancellationToken ct)`
  - Issues a reset ticket and records it within Supabase for secure delivery.
- `GetAuditTrailAsync(Guid organizationId, CancellationToken ct)`
  - Fetches recent security events for admin review and compliance.

**Implementation Notes**
- Uses Supabase PostgREST tables for `user_accounts`, `user_roles`, and `audit_logs`.
- All mutating operations produce audit entries to keep a traceable history.
- Designed for integration with admin UI to expose activation, suspension, and password flows.

---

### ISettingsCustomizationService - Personalization and Feature Flags

**Purpose**: Provides APIs to load and persist organization-wide and user-specific personalization, including dashboard widgets and KPI snapshots.

**Namespace**: `NexaCRM.WebClient.Services.Interfaces`

#### Key Methods
- `GetOrganizationSettingsAsync(Guid organizationId, CancellationToken ct)` / `SaveOrganizationSettingsAsync(...)`
  - Manages localization, timezone, and organization-level feature flags.
- `GetUserPreferencesAsync(Guid userId, CancellationToken ct)` / `SaveUserPreferencesAsync(...)`
  - Tracks per-user theme, notification preferences, and widget metadata.
- `GetDashboardLayoutAsync(Guid userId, CancellationToken ct)` / `SaveDashboardLayoutAsync(...)`
  - Supports configurable widget ordering and layout persistence.
- `GetKpiSnapshotsAsync(string metric, DateTime since, CancellationToken ct)`
  - Retrieves historical KPI values for dashboard charts.

**Implementation Notes**
- Persists data in `organization_settings`, `user_preferences`, `dashboard_widgets`, and `kpi_snapshots` Supabase tables.
- Uses JSON serialization to flexibly capture dynamic widget and flag metadata.
- Built to back advanced dashboard customization screens within the admin area.

---

### IFileHubService - Storage, Versioning, and Threaded Collaboration

**Purpose**: Coordinates file upload flows, metadata registration, version history, and threaded communications for attached documents.

**Namespace**: `NexaCRM.WebClient.Services.Interfaces`

#### Key Methods
- `CreateUploadUrlAsync(FileUploadRequest request, CancellationToken ct)`
  - Generates signed Supabase Storage URLs with the correct headers and path scoping.
- `RegisterUploadAsync(Guid userId, string objectPath, FileUploadRequest request, CancellationToken ct)`
  - Stores file metadata and creates the first version entry.
- `GetFileVersionsAsync(Guid fileId, CancellationToken ct)`
  - Returns ordered version history for auditability.
- `EnsureThreadAsync(string entityType, string entityId, string channel, CancellationToken ct)`
  - Lazily creates or loads a communication thread per entity and channel.
- `SendThreadMessageAsync(Guid threadId, Guid senderId, string body, IEnumerable<string> channels, CancellationToken ct)`
  - Records a message and dispatches integration events for downstream email/SMS delivery.

**Implementation Notes**
- Storage paths follow a deterministic convention allowing RLS policies to guard access per entity.
- Integrates with Supabase `file_documents`, `file_versions`, `communication_threads`, and `thread_messages` tables.
- Automatically logs file events into the shared `audit_logs` stream.

---

### ICommunicationHubService - Email, SMS, and Push Abstraction

**Purpose**: Centralizes queuing of multi-channel outbound communications for asynchronous processing.

**Key Methods**
- `SendEmailAsync(Guid senderId, IEnumerable<string> recipients, string subject, string body, CancellationToken ct)`
- `SendSmsAsync(Guid senderId, IEnumerable<string> recipients, string message, CancellationToken ct)`
- `EnqueuePushNotificationAsync(Guid userId, string title, string message, CancellationToken ct)`

**Implementation Notes**
- Creates integration events (`communication.email`, `communication.sms`, `notification.push`) consumed by worker services.
- Performs validation on recipients and ensures duplicate suppression per request.

---

### ISyncOrchestrationService - Offline Cache and Conflict Resolution

**Purpose**: Supplies server-generated sync plans, accepts client deltas, and coordinates conflict resolution for offline-capable clients.

**Key Methods**
- `BuildSyncPlanAsync(Guid userId, SyncPolicy policy, CancellationToken ct)`
  - Produces ordered payloads for entities that changed since the last refresh.
- `RecordClientEnvelopeAsync(SyncEnvelope envelope, CancellationToken ct)`
  - Stores client-side updates and attaches them to sync envelopes.
- `GetConflictsAsync(Guid userId, CancellationToken ct)` / `ResolveConflictsAsync(...)`
  - Surfaces pending conflicts and records their resolution strategies.

**Implementation Notes**
- Utilizes Supabase tables `sync_envelopes`, `sync_items`, and `sync_conflicts`.
- Emits `sync.conflict.resolved` integration events so background workers can reconcile server data.
- Designed for progressive enhancement: policies control refresh cadence and entity scope for constrained devices.

**UpdateTaskAsync(Models.Task task)**
- **Purpose**: Updates an existing task with new information
- **Return Type**: `Task` (void)
- **Parameters**: 
  - `task` (Models.Task): Task object with updated properties
- **Validation**: Task ID must exist, user must have update permission
- **Exceptions**: `TaskNotFoundException`, `ValidationException`, `UnauthorizedAccessException`

**DeleteTaskAsync(int id)**
- **Purpose**: Removes a task from the system
- **Return Type**: `Task` (void)
- **Parameters**: 
  - `id` (int): Unique task identifier to delete
- **Exceptions**: `TaskNotFoundException`, `UnauthorizedAccessException`

#### Usage Example
```csharp
@inject ITaskService TaskService

// Create a new task
private async Task CreateNewTask()
{
    var newTask = new Models.Task
    {
        Title = "Follow up with client",
        Description = "Schedule meeting to discuss requirements",
        Priority = TaskPriority.High,
        DueDate = DateTime.Now.AddDays(2),
        AssignedTo = currentUser.Id
    };
    
    await TaskService.CreateTaskAsync(newTask);
}

// Update task status
private async Task CompleteTask(Models.Task task)
{
    task.Status = TaskStatus.Completed;
    task.CompletedDate = DateTime.Now;
    
    await TaskService.UpdateTaskAsync(task);
}
```

#### Implementation Notes
- **Current Implementation**: MockTaskService with in-memory storage
- **Future Enhancement**: Will include task dependencies, subtasks, and recurring tasks
- **Notifications**: Task changes will trigger notification events
- **Audit Trail**: All task modifications will be logged for audit purposes

---

### ISupportTicketService - Support Ticket Management Interface

**Purpose**: Customer support ticket lifecycle management with live interaction capabilities

**Namespace**: `NexaCRM.WebClient.Services.Interfaces`

#### Interface Definition
```csharp
public interface ISupportTicketService
{
    Task<IEnumerable<SupportTicket>> GetTicketsAsync();
    Task<SupportTicket> GetTicketByIdAsync(int id);
    Task<IEnumerable<SupportTicket>> GetLiveInteractionsAsync();
    Task CreateTicketAsync(SupportTicket ticket);
    Task UpdateTicketAsync(SupportTicket ticket);
    Task DeleteTicketAsync(int id);
}
```

#### Method Details

**GetTicketsAsync()**
- **Purpose**: Retrieves all support tickets accessible to the current user
- **Return Type**: `Task<IEnumerable<SupportTicket>>`
- **Parameters**: None
- **Security**: Filters tickets based on user role and permissions

**GetTicketByIdAsync(int id)**
- **Purpose**: Retrieves detailed information for a specific support ticket
- **Return Type**: `Task<SupportTicket>`
- **Parameters**: 
  - `id` (int): Unique ticket identifier
- **Includes**: Ticket history, attachments, and related conversations

**GetLiveInteractionsAsync()**
- **Purpose**: Retrieves tickets with active, real-time customer interactions
- **Return Type**: `Task<IEnumerable<SupportTicket>>`
- **Parameters**: None
- **Filtering**: Returns only tickets with recent customer activity or agent responses

**CreateTicketAsync(SupportTicket ticket)**
- **Purpose**: Creates a new support ticket in the system
- **Return Type**: `Task` (void)
- **Parameters**: 
  - `ticket` (SupportTicket): Ticket object with customer information and issue details
- **Auto-Assignment**: May automatically assign to available agents based on category

**UpdateTicketAsync(SupportTicket ticket)**
- **Purpose**: Updates ticket status, priority, or assignment
- **Return Type**: `Task` (void)
- **Parameters**: 
  - `ticket` (SupportTicket): Updated ticket object
- **Notifications**: Status changes trigger customer notifications

**DeleteTicketAsync(int id)**
- **Purpose**: Archives or removes a support ticket
- **Return Type**: `Task` (void)
- **Parameters**: 
  - `id` (int): Ticket identifier to delete
- **Note**: Typically archives rather than permanently deletes for audit trail

#### Usage Example
```csharp
@inject ISupportTicketService SupportTicketService

// Handle live interactions
private async Task LoadLiveInteractions()
{
    var liveTickets = await SupportTicketService.GetLiveInteractionsAsync();
    
    foreach (var ticket in liveTickets)
    {
        // Display real-time ticket updates
        DisplayTicketInLiveDashboard(ticket);
    }
}

// Escalate ticket
private async Task EscalateTicket(SupportTicket ticket)
{
    ticket.Priority = TicketPriority.Critical;
    ticket.AssignedTo = supervisorId;
    
    await SupportTicketService.UpdateTicketAsync(ticket);
}
```

#### Implementation Notes
- **Real-Time Features**: Supports live updates and customer chat integration
- **SLA Tracking**: Monitors response times and resolution metrics
- **Knowledge Base Integration**: Links to relevant knowledge base articles
- **Multi-Channel Support**: Handles tickets from email, chat, phone, and web forms

---

### IAgentService - Agent Management Interface

**Purpose**: Sales and support agent roster management and assignment

**Namespace**: `NexaCRM.WebClient.Services.Interfaces`

#### Interface Definition
```csharp
public interface IAgentService
{
    Task<IEnumerable<Agent>> GetAgentsAsync();
}
```

#### Method Details

**GetAgentsAsync()**
- **Purpose**: Retrieves all active agents in the system
- **Return Type**: `Task<IEnumerable<Agent>>`
- **Parameters**: None
- **Filtering**: Returns active agents based on current user's visibility permissions

#### Usage Example
```csharp
@inject IAgentService AgentService

private async Task LoadAvailableAgents()
{
    var agents = await AgentService.GetAgentsAsync();
    var availableAgents = agents.Where(a => a.Status == AgentStatus.Available);
}
```

#### Implementation Notes
- **Current Implementation**: MockAgentService with sample agent data
- **Future Enhancement**: Will include agent availability, skills, and performance metrics
- **Assignment Logic**: Used for automatic ticket and lead assignment
- **Performance Tracking**: Agent productivity and customer satisfaction metrics

---

### IMarketingCampaignService - Marketing Campaign Management Interface

**Purpose**: Marketing campaign lifecycle management with full CRUD operations

**Namespace**: `NexaCRM.WebClient.Services.Interfaces`

#### Interface Definition
```csharp
public interface IMarketingCampaignService
{
    Task<IEnumerable<MarketingCampaign>> GetCampaignsAsync();
    Task<MarketingCampaign> GetCampaignByIdAsync(int id);
    Task CreateCampaignAsync(MarketingCampaign campaign);
    Task UpdateCampaignAsync(MarketingCampaign campaign);
    Task DeleteCampaignAsync(int id);
}
```

#### Method Details

**GetCampaignsAsync()**
- **Purpose**: Retrieves all marketing campaigns accessible to the current user
- **Return Type**: `Task<IEnumerable<MarketingCampaign>>`
- **Parameters**: None
- **Ordering**: Typically ordered by creation date (newest first)

**GetCampaignByIdAsync(int id)**
- **Purpose**: Retrieves detailed information for a specific campaign
- **Return Type**: `Task<MarketingCampaign>`
- **Parameters**: 
  - `id` (int): Unique campaign identifier
- **Includes**: Campaign metrics, target audience, and performance data

**CreateCampaignAsync(MarketingCampaign campaign)**
- **Purpose**: Creates a new marketing campaign
- **Return Type**: `Task` (void)
- **Parameters**: 
  - `campaign` (MarketingCampaign): Campaign object with configuration details
- **Validation**: Campaign name, target audience, and budget are required

**UpdateCampaignAsync(MarketingCampaign campaign)**
- **Purpose**: Updates existing campaign configuration or status
- **Return Type**: `Task` (void)
- **Parameters**: 
  - `campaign` (MarketingCampaign): Updated campaign object
- **Restrictions**: Active campaigns may have limited update capabilities

**DeleteCampaignAsync(int id)**
- **Purpose**: Archives or removes a marketing campaign
- **Return Type**: `Task` (void)
- **Parameters**: 
  - `id` (int): Campaign identifier to delete
- **Note**: Active campaigns must be stopped before deletion

#### Usage Example
```csharp
@inject IMarketingCampaignService MarketingCampaignService

// Launch new campaign
private async Task LaunchCampaign()
{
    var campaign = new MarketingCampaign
    {
        Name = "Q4 Product Launch",
        Type = CampaignType.Email,
        Budget = 50000,
        StartDate = DateTime.Now.AddDays(7),
        EndDate = DateTime.Now.AddDays(37),
        TargetAudience = "Enterprise Customers"
    };
    
    await MarketingCampaignService.CreateCampaignAsync(campaign);
}
```

#### Implementation Notes
- **Campaign Types**: Supports email, social media, PPC, and multi-channel campaigns
- **Analytics Integration**: Tracks ROI, conversion rates, and engagement metrics
- **Automation**: Integrates with marketing automation workflows
- **A/B Testing**: Supports campaign variations and performance comparison

---

### IReportService - Reporting and Analytics Interface

**Purpose**: Business intelligence and analytics reporting with multiple report types

**Namespace**: `NexaCRM.WebClient.Services.Interfaces`

#### Interface Definition
```csharp
public interface IReportService
{
    Task SaveReportDefinitionAsync(ReportDefinition definition);
    Task<IEnumerable<ReportDefinition>> GetReportDefinitionsAsync();
    Task<ReportData> GenerateReportAsync(ReportDefinition definition);
    Task<ReportData> GetQuarterlyPerformanceAsync();
    Task<ReportData> GetLeadSourceAnalyticsAsync();
    Task<ReportData> GetTicketVolumeAsync();
    Task<ReportData> GetResolutionRateAsync();
    Task<ReportData> GetTicketsByCategoryAsync();
}
```

#### Method Details

**GetQuarterlyPerformanceAsync()**
- **Purpose**: Generates quarterly sales and performance report
- **Return Type**: `Task<ReportData>`
- **Parameters**: None (uses current quarter by default)
- **Includes**: Revenue, deal closure rates, and performance trends

**GetLeadSourceAnalyticsAsync()**
- **Purpose**: Analyzes lead generation sources and conversion effectiveness
- **Return Type**: `Task<ReportData>`
- **Parameters**: None
- **Includes**: Lead sources, conversion rates, and ROI by channel

**SaveReportDefinitionAsync(ReportDefinition)**
- **Purpose**: Persists a custom report definition
- **Return Type**: `Task`
- **Parameters**: `ReportDefinition` definition

**GetReportDefinitionsAsync()**
- **Purpose**: Retrieves saved report definitions
- **Return Type**: `Task<IEnumerable<ReportDefinition>>`
- **Parameters**: None

**GenerateReportAsync(ReportDefinition)**
- **Purpose**: Generates report data based on a definition
- **Return Type**: `Task<ReportData>`
- **Parameters**: `ReportDefinition` definition

**GetTicketVolumeAsync()**
- **Purpose**: Reports on support ticket volume and trends
- **Return Type**: `Task<ReportData>`
- **Parameters**: None
- **Includes**: Ticket counts, volume trends, and peak times

**GetResolutionRateAsync()**
- **Purpose**: Analyzes support ticket resolution performance
- **Return Type**: `Task<ReportData>`
- **Parameters**: None
- **Includes**: Average resolution time, SLA compliance, and agent performance

**GetTicketsByCategoryAsync()**
- **Purpose**: Categorizes support tickets by type and priority
- **Return Type**: `Task<ReportData>`
- **Parameters**: None
- **Includes**: Category distribution, priority levels, and trend analysis

#### Usage Example
```csharp
@inject IReportService ReportService

private async Task LoadDashboardReports()
{
    var quarterlyData = await ReportService.GetQuarterlyPerformanceAsync();
    var leadAnalytics = await ReportService.GetLeadSourceAnalyticsAsync();
    var ticketMetrics = await ReportService.GetTicketVolumeAsync();
    
    // Display reports in dashboard
    DisplayQuarterlyChart(quarterlyData);
    DisplayLeadSourceChart(leadAnalytics);
    DisplayTicketTrends(ticketMetrics);
}
```

#### Implementation Notes
- **Real-Time Data**: Reports reflect current data with configurable refresh intervals
- **Export Capabilities**: All reports support PDF and CSV export formats
- **Drill-Down**: Interactive reports allow drilling down into detailed data
- **Scheduling**: Reports can be scheduled for automatic generation and delivery

---

### IActivityService - Activity Logging Interface

**Purpose**: System-wide activity tracking and audit trail management

**Namespace**: `NexaCRM.WebClient.Services.Interfaces`

#### Interface Definition
```csharp
public interface IActivityService
{
    Task<IEnumerable<Activity>> GetRecentActivitiesAsync();
}
```

#### Method Details

**GetRecentActivitiesAsync()**
- **Purpose**: Retrieves recent system activities and user actions
- **Return Type**: `Task<IEnumerable<Activity>>`
- **Parameters**: None (returns last 50 activities by default)
- **Ordering**: Most recent activities first
- **Filtering**: Respects user permissions and data visibility rules

#### Usage Example
```csharp
@inject IActivityService ActivityService

private async Task LoadActivityFeed()
{
    var activities = await ActivityService.GetRecentActivitiesAsync();
    
    foreach (var activity in activities)
    {
        DisplayActivityItem(activity);
    }
}
```

#### Implementation Notes
- **Auto-Logging**: System automatically logs significant user actions
- **Performance**: Optimized for frequent reads with minimal impact
- **Privacy**: Respects user privacy settings and data access permissions
- **Integration**: All services can log activities through this interface

---

### CustomAuthStateProvider - Authentication Management

**Purpose**: User session and authentication state management with role-based access control

**Namespace**: `NexaCRM.WebClient.Services`

#### Class Definition
```csharp
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync();
    public void UpdateAuthenticationState(string username, string[] roles);
    public void Logout();
}
```

#### Method Details

**GetAuthenticationStateAsync()**
- **Purpose**: Retrieves the current user's authentication state
- **Return Type**: `Task<AuthenticationState>`
- **Parameters**: None
- **Returns**: Authentication state with user identity and claims

**UpdateAuthenticationState(string username, string[] roles)**
- **Purpose**: Updates the authentication state with new user information
- **Return Type**: void
- **Parameters**: 
  - `username` (string): User's login name
  - `roles` (string[]): Array of user roles for authorization
- **Notifications**: Triggers authentication state change events

**Logout()**
- **Purpose**: Clears the current user session and resets authentication state
- **Return Type**: void
- **Parameters**: None
- **Side Effects**: Redirects to login page and clears cached user data

#### Usage Example
```csharp
@inject CustomAuthStateProvider AuthStateProvider

private async Task HandleLogin(string username, string password)
{
    // Validate credentials (mock implementation)
    if (ValidateCredentials(username, password))
    {
        var roles = GetUserRoles(username);
        AuthStateProvider.UpdateAuthenticationState(username, roles);
    }
}

private void HandleLogout()
{
    AuthStateProvider.Logout();
}
```

#### Implementation Notes
- **Current Implementation**: Mock authentication with configurable roles
- **Future Migration**: Will integrate with Supabase authentication
- **Session Management**: Handles session persistence and refresh
- **Security**: Implements proper token handling and validation

## Implementation Guidelines

### Best Practices for Service Implementation

#### Dependency Injection Pattern
```csharp
public class ExampleService : IExampleService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExampleService> _logger;
    
    public ExampleService(HttpClient httpClient, ILogger<ExampleService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

#### Async/Await Usage Guidelines
```csharp
public async Task<IEnumerable<Contact>> GetContactsAsync()
{
    try
    {
        _logger.LogInformation("Fetching contacts for user {UserId}", _currentUser.Id);
        
        var response = await _httpClient.GetAsync("/api/contacts");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var contacts = JsonSerializer.Deserialize<IEnumerable<Contact>>(content);
        
        _logger.LogInformation("Successfully fetched {Count} contacts", contacts.Count());
        return contacts;
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "HTTP error while fetching contacts");
        throw new ServiceException("Failed to retrieve contacts", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error while fetching contacts");
        throw;
    }
}
```

#### Error Handling Patterns
```csharp
public class ServiceException : Exception
{
    public ServiceException(string message) : base(message) { }
    public ServiceException(string message, Exception innerException) : base(message, innerException) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public Dictionary<string, string> ValidationErrors { get; set; } = new();
}
```

### Testing Strategies for Services

#### Unit Testing with Mocks
```csharp
[Test]
public async Task GetContactsAsync_ReturnsContacts_WhenDataExists()
{
    // Arrange
    var mockHttpClient = new Mock<HttpClient>();
    var mockLogger = new Mock<ILogger<ContactService>>();
    var service = new ContactService(mockHttpClient.Object, mockLogger.Object);
    
    // Act
    var result = await service.GetContactsAsync();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.IsTrue(result.Any());
}
```

#### Integration Testing
```csharp
[Test]
public async Task ContactService_IntegrationTest_WithRealDatabase()
{
    // Arrange
    using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    var contactService = factory.Services.GetRequiredService<IContactService>();
    
    // Act
    var contacts = await contactService.GetContactsAsync();
    
    // Assert
    Assert.IsNotNull(contacts);
}
```

#### Performance Testing
```csharp
[Test]
[Timeout(5000)] // 5 second timeout
public async Task GetContactsAsync_PerformanceTest_CompletesWithinTimeout()
{
    // Arrange
    var service = CreateContactService();
    
    // Act & Assert
    var stopwatch = Stopwatch.StartNew();
    var result = await service.GetContactsAsync();
    stopwatch.Stop();
    
    Assert.IsTrue(stopwatch.ElapsedMilliseconds < 2000, "Service call took too long");
}
```

## Error Handling Reference

### Common Exceptions

| Exception Type | When to Use | Example |
|---------------|-------------|---------|
| `ServiceException` | General service errors | Network timeouts, API errors |
| `ValidationException` | Input validation failures | Required field missing, invalid format |
| `UnauthorizedAccessException` | Permission denied | User lacks required role |
| `NotFoundException` | Resource not found | Contact ID doesn't exist |
| `ConflictException` | Resource conflicts | Duplicate email address |

### Exception Handling Best Practices
```csharp
try
{
    await service.CreateContactAsync(contact);
}
catch (ValidationException ex)
{
    // Display field-specific validation errors
    DisplayValidationErrors(ex.ValidationErrors);
}
catch (UnauthorizedAccessException)
{
    // Redirect to login or show permission denied
    NavigateToLogin();
}
catch (ServiceException ex)
{
    // Show user-friendly error message
    ShowErrorNotification("Unable to save contact. Please try again.");
    
    // Log technical details
    _logger.LogError(ex, "Failed to create contact");
}
```

## Performance Considerations

### Caching Strategies
- **Memory Caching**: For frequently accessed, relatively static data
- **HTTP Caching**: For API responses with appropriate cache headers
- **User-Specific Caching**: Cache user-specific data separately

### Pagination Patterns
```csharp
public interface IPagedResult<T>
{
    IEnumerable<T> Items { get; }
    int TotalCount { get; }
    int PageNumber { get; }
    int PageSize { get; }
    bool HasNextPage { get; }
    bool HasPreviousPage { get; }
}

// Future enhanced interface
public interface IContactService
{
    Task<IPagedResult<Contact>> GetContactsAsync(int pageNumber = 1, int pageSize = 50);
}
```

### Batch Operations
```csharp
// Future enhanced interfaces
public interface ITaskService
{
    Task CreateTasksAsync(IEnumerable<Models.Task> tasks); // Batch create
    Task UpdateTasksAsync(IEnumerable<Models.Task> tasks); // Batch update
    Task DeleteTasksAsync(IEnumerable<int> taskIds);       // Batch delete
}
```

## Security Considerations

### Authentication Requirements
- All service methods require authenticated user context
- Role-based authorization enforced at service layer
- Sensitive operations require additional permission checks

### Data Protection
- Personal data access logged for audit compliance
- Data encryption in transit and at rest
- GDPR compliance for data handling and deletion

### API Security
- All API calls use HTTPS encryption
- Authentication tokens have appropriate expiration
- Rate limiting applied to prevent abuse

## Related Documentation

- [WEB_CLIENT_DOCUMENTATION.md](./WEB_CLIENT_DOCUMENTATION.md) - Complete web client architecture guide
- [API_INTEGRATION_ROADMAP.md](./API_INTEGRATION_ROADMAP.md) - Migration strategy from mock to real services
- [FRONTEND_INTEGRATION.md](./FRONTEND_INTEGRATION.md) - Supabase integration implementation guide
- [README.md](./README.md) - Project overview and getting started guide