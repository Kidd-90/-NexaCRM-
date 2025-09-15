# NexaCRM Web Client Documentation

## Project Overview

### Architecture Overview

NexaCRM Web Client is a modern customer relationship management (CRM) web application built with Blazor WebAssembly technology. The application follows a service-oriented architecture with clean separation between presentation, business logic, and data access layers.

**Key Architectural Principles:**
- **Client-Side Architecture**: Blazor WebAssembly for rich, interactive client-side web applications
- **Service Layer Pattern**: Well-defined service interfaces with dependency injection
- **Mock-First Development**: All services currently use mock implementations for rapid prototyping
- **Authentication State Management**: Custom authentication state provider for user session management
- **Localization Support**: Built-in Korean language support with extensible localization framework

### Project Structure

```
src/Web/NexaCRM.WebClient/
├── Pages/                    # Blazor page components
├── Shared/                   # Shared components and layouts
├── Services/                 # Service layer implementation
│   ├── Interfaces/          # Service interface definitions
│   ├── Mock/                # Mock service implementations
│   └── CustomAuthStateProvider.cs
├── Models/                   # Data models and DTOs
├── Resources/                # Localization resources
├── Styles/                   # CSS and styling files
├── wwwroot/                  # Static web assets
├── App.razor                 # Root application component
├── Program.cs                # Application entry point and DI configuration
└── _Imports.razor            # Global using statements
```

### Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| **Frontend Framework** | Blazor WebAssembly | .NET 8 |
| **Runtime** | .NET | 8.0 |
| **Backend Integration** | Supabase | (Planned) |
| **Authentication** | Custom Auth State Provider | Current |
| **Styling** | CSS3, Bootstrap | Latest |
| **Localization** | .NET Resource System | Built-in |
| **Build Tool** | MSBuild | .NET 8 SDK |
| **IDE Support** | Visual Studio 2022, VS Code | Latest |

### Key Features and Capabilities

**Current Implemented Features:**
- **Contact Management**: Comprehensive customer contact tracking and management
- **Deal Pipeline**: Sales opportunity management with pipeline visualization
- **Task Management**: Full CRUD operations for task tracking and assignment
- **Support Ticketing**: Customer support ticket management with live interaction capabilities
- **Agent Management**: Sales and support agent administration
- **Marketing Campaigns**: Campaign creation, management, and tracking
- **Reporting & Analytics**: Multiple report types including performance, lead analytics, and ticket metrics
- **Activity Logging**: Comprehensive activity tracking and audit trail
- **Authentication**: User session management with role-based access
- **Responsive Design**: Mobile-friendly interface with adaptive layouts
- **Internationalization**: Korean language support with extensible localization

## Service Architecture

### Currently Implemented Services

The application implements a comprehensive service layer with the following interfaces and their responsibilities:

#### 1. IContactService - Contact Management
**Purpose**: Manages customer contact information and interactions
```csharp
Task<IEnumerable<Contact>> GetContactsAsync();
```
**Capabilities**: Basic contact retrieval and management

#### 2. IDealService - Deal/Opportunity Management  
**Purpose**: Handles sales opportunities and deal pipeline management
```csharp
Task<IEnumerable<Deal>> GetDealsAsync();
```
**Capabilities**: Deal tracking and pipeline visualization

#### 3. ITaskService - Task Management with CRUD Operations
**Purpose**: Complete task lifecycle management
```csharp
Task<IEnumerable<Task>> GetTasksAsync();
Task<Task> GetTaskByIdAsync(int id);
Task CreateTaskAsync(Task task);
Task UpdateTaskAsync(Task task);
Task DeleteTaskAsync(int id);
```
**Capabilities**: Full CRUD operations for task management, assignment, and tracking

#### 4. ISupportTicketService - Support Ticket Management with Live Interactions
**Purpose**: Customer support ticket lifecycle and real-time interactions
```csharp
Task<IEnumerable<SupportTicket>> GetTicketsAsync();
Task<SupportTicket> GetTicketByIdAsync(int id);
Task<IEnumerable<SupportTicket>> GetLiveInteractionsAsync();
Task CreateTicketAsync(SupportTicket ticket);
Task UpdateTicketAsync(SupportTicket ticket);
Task DeleteTicketAsync(int id);
```
**Capabilities**: Complete ticket management with live interaction support

#### 5. IAgentService - Agent Management
**Purpose**: Sales and support agent administration
```csharp
Task<IEnumerable<Agent>> GetAgentsAsync();
```
**Capabilities**: Agent roster management and assignment

#### 6. IMarketingCampaignService - Marketing Campaign Management with CRUD
**Purpose**: Marketing campaign lifecycle management
```csharp
Task<IEnumerable<MarketingCampaign>> GetCampaignsAsync();
Task<MarketingCampaign> GetCampaignByIdAsync(int id);
Task CreateCampaignAsync(MarketingCampaign campaign);
Task UpdateCampaignAsync(MarketingCampaign campaign);
Task DeleteCampaignAsync(int id);
```
**Capabilities**: Complete campaign management with tracking and analytics

#### 7. IReportService - Reporting Service with Multiple Report Types
**Purpose**: Business intelligence and analytics reporting
```csharp
Task<ReportData> GetQuarterlyPerformanceAsync();
Task<ReportData> GetLeadSourceAnalyticsAsync();
Task<ReportData> GetTicketVolumeAsync();
Task<ReportData> GetResolutionRateAsync();
Task<ReportData> GetTicketsByCategoryAsync();
```
**Capabilities**: Comprehensive reporting covering sales, marketing, and support metrics

#### 8. IActivityService - Activity Logging Service
**Purpose**: System-wide activity tracking and audit logging
```csharp
Task<IEnumerable<Activity>> GetRecentActivitiesAsync();
```
**Capabilities**: Activity feed and audit trail management

#### 9. CustomAuthStateProvider - Authentication Management
**Purpose**: User session and authentication state management
```csharp
Task<AuthenticationState> GetAuthenticationStateAsync();
void UpdateAuthenticationState(string username, string[] roles);
void Logout();
```
**Capabilities**: Custom authentication with role-based access control

### Mock Services Implementation

All services currently use mock implementations to enable rapid development and testing:

**Mock Data Characteristics:**
- **Realistic Data**: Mock services provide realistic sample data for all entities
- **CRUD Simulation**: Full create, read, update, delete operations where applicable
- **In-Memory Storage**: Data persists for the session but resets on application restart
- **Performance**: Async/await patterns maintained for future API compatibility
- **Error Simulation**: Capability to simulate various error conditions for testing

**Service Registration in Program.cs:**
```csharp
builder.Services.AddScoped<IContactService, MockContactService>();
builder.Services.AddScoped<IDealService, MockDealService>();
builder.Services.AddScoped<ITaskService, MockTaskService>();
builder.Services.AddScoped<ISupportTicketService, MockSupportTicketService>();
builder.Services.AddScoped<IAgentService, MockAgentService>();
builder.Services.AddScoped<IMarketingCampaignService, MockMarketingCampaignService>();
builder.Services.AddScoped<IReportService, MockReportService>();
builder.Services.AddScoped<IActivityService, MockActivityService>();
```

## Missing Services Analysis

### High Priority Missing Services

#### Authentication & Security Services
- **IUserManagementService**: User account creation, modification, and management
- **IRolePermissionService**: Role-based access control and permission management
- **IPasswordResetService**: Password reset workflows and security
- **ISecurityService**: Security policies, audit logs, and compliance

#### Configuration Management
- **ISettingsService**: Application settings and user preferences
- **IConfigurationService**: System configuration and feature flags
- **IThemeService**: UI themes and customization options

#### Dashboard & Analytics
- **IDashboardService**: Dashboard widget management and personalization
- **IAnalyticsService**: Advanced analytics and business intelligence
- **IKPIService**: Key Performance Indicator tracking and monitoring

### Medium Priority Missing Services

#### File Management
- **IFileUploadService**: File upload and storage management
- **IDocumentService**: Document management and versioning
- **IAttachmentService**: File attachments for tickets, deals, and contacts

#### Communication
- **INotificationService**: In-app and push notifications
- **IEmailService**: Email integration and template management
- **ICommunicationService**: Multi-channel communication coordination

### Low Priority Missing Services

#### Data Synchronization
- **ISyncService**: Data synchronization across devices and sessions
- **ICacheService**: Caching strategies for performance optimization
- **IOfflineService**: Offline functionality and data persistence

## Pages and Components

### Current Page Implementations

**Core Business Pages:**
- **MainDashboard.razor**: Primary dashboard with KPI overview and quick actions
- **ContactsPage.razor**: Contact management interface with search and filtering
- **SalesPipelinePage.razor**: Visual deal pipeline with drag-and-drop functionality
- **TasksPage.razor**: Task management interface with assignment and tracking
- **ReportsPage.razor**: Custom report builder with field selection, filters, saved definitions, preview, and mobile-friendly layout

**Support and Service Pages:**
- **CustomerSupportDashboard.razor**: Support team dashboard with ticket overview
- **CustomerSupportTicketManagementInterface.razor**: Detailed ticket management
- **CustomerSupportKnowledgeBase.razor**: Knowledge base for support resources

**Marketing and Sales Pages:**
- **MarketingCampaignManagementInterface.razor**: Campaign creation and management
- **SalesManagerDashboard.razor**: Sales team performance and pipeline analytics
- **SalesTeamGoalSettingAndTrackingInterface.razor**: Goal setting and progress tracking

**Administrative Pages:**
- **SettingsPage.razor**: Application settings and configuration
- **PasswordResetPage.razor**: Password reset functionality
- **ProfileSettingsPage.razor**: User profile management
- **UserRegistrationPage.razor**: New user registration

**Specialized Tools:**
- **EmailTemplateBuilder.razor**: Email template creation and management
- **CustomerFeedbackAndSurveyManagementTool.razor**: Survey and feedback management
- **CustomerSegmentationTool.razor**: Customer segmentation and analytics

### Navigation System and Responsive Design

**Navigation Features:**
- **Responsive Layout**: Adaptive navigation for desktop, tablet, and mobile devices
- **Role-Based Menus**: Dynamic menu items based on user permissions
- **Breadcrumb Navigation**: Hierarchical navigation with context awareness
- **Quick Access**: Shortcuts to frequently used functions

**Design Principles:**
- **Mobile-First**: Responsive design prioritizing mobile user experience
- **Accessibility**: WCAG 2.1 AA compliance for inclusive design
- **Performance**: Optimized for fast loading and smooth interactions
- **Consistency**: Unified design language across all components

### Localization Support

**Current Localization:**
- **Korean Language**: Primary localization with comprehensive coverage
- **Resource System**: .NET standard resource files for text management
- **Cultural Formatting**: Proper date, number, and currency formatting for Korean locale

**Extensibility:**
- **Multi-Language Ready**: Architecture supports additional languages
- **Resource Management**: Centralized resource files for easy translation
- **RTL Support**: Framework ready for right-to-left languages

## Development Priorities

### High Priority Development Tasks

1. **User Management Implementation**
   - Replace mock authentication with production-ready user management
   - Implement role-based access control (RBAC)
   - Add user registration, profile management, and password reset workflows

2. **Settings and Configuration**
   - Build comprehensive settings management system
   - Implement user preferences and customization options
   - Add system configuration and feature flag management

3. **Enhanced Dashboard Services**
   - Develop real-time dashboard with live data updates
   - Implement customizable widgets and layouts
   - Add advanced analytics and KPI tracking

### Medium Priority Development Tasks

1. **Notification System**
   - Implement in-app notification service
   - Add email notification integration
   - Build notification preferences and management

2. **File Management**
   - Add file upload and storage capabilities
   - Implement document management with versioning
   - Build attachment system for entities

3. **Advanced Analytics**
   - Enhance reporting with interactive charts and graphs
   - Add predictive analytics and trend analysis
   - Implement custom report builder

### Low Priority Development Tasks

1. **Data Synchronization**
   - Implement real-time data sync across sessions
   - Add intelligent caching for performance
   - Build offline functionality with conflict resolution

2. **Advanced Integration**
   - Add third-party CRM integrations
   - Implement API webhook management
   - Build custom integration framework

## Development Guidelines

### Code Standards
- Follow C# coding conventions and best practices
- Use dependency injection for service management
- Implement async/await patterns consistently
- Maintain comprehensive XML documentation

### Testing Strategy
- Unit tests for all service implementations
- Integration tests for critical workflows
- End-to-end tests for user scenarios
- Performance tests for scalability validation

### Security Best Practices
- Input validation and sanitization
- Secure authentication and session management
- Regular security audits and penetration testing
- Compliance with data protection regulations

## Related Documentation

- [README.md](./README.md) - Project overview and setup instructions (Korean)
- [FRONTEND_INTEGRATION.md](./FRONTEND_INTEGRATION.md) - Supabase integration guide
- [API_INTEGRATION_ROADMAP.md](./API_INTEGRATION_ROADMAP.md) - API migration planning
- [SERVICE_INTERFACES_REFERENCE.md](./SERVICE_INTERFACES_REFERENCE.md) - Complete service interface reference