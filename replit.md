# NexaCRM - Replit Project Setup

## Overview
NexaCRM is a comprehensive CRM (Customer Relationship Management) solution built with C# Blazor WebAssembly (.NET 8) for the frontend and ASP.NET Core microservices for the backend. This project provides customer management, sales pipeline tracking, workflow automation, and reporting capabilities.

## Recent Changes (Setup Session - September 19, 2025)
- ✅ Installed .NET 8.0 development environment
- ✅ Configured Blazor WebAssembly application to run on port 5000 with 0.0.0.0 binding
- ✅ Set up Python HTTP server workflow as workaround for .NET development server issues
- ✅ Configured deployment settings for production (autoscale target)
- ⚠️ Note: .NET build process has issues in current Replit environment - using static file serving

## Project Architecture
The solution follows a microservices architecture with:

### Frontend
- **Technology**: Blazor WebAssembly (.NET 8)
- **Location**: `src/Web/NexaCRM.WebClient/`
- **Features**: 
  - Multi-language support (Korean/English)
  - Responsive design for desktop/tablet/mobile
  - Mock services for development testing
  - Authentication system with JWT

### Backend Services
- **API Gateway**: `src/ApiGateway/` - Routes requests to microservices
- **Contact API**: `src/Services/Contact.API/` - Contact management
- **Deal API**: `src/Services/Deal.API/` - Sales pipeline management  
- **Identity API**: `src/Services/Identity.API/` - Authentication & authorization

### Building Blocks
- **Common**: `src/BuildingBlocks/Common/` - Shared utilities
- **EventBus**: `src/BuildingBlocks/EventBus/` - Message broker integration

## Current Configuration

### Workflow Setup
- **Name**: NexaCRM Frontend
- **Command**: `cd src/Web/NexaCRM.WebClient/wwwroot && python -m http.server 5000 --bind 0.0.0.0`
- **Port**: 5000 (webview)
- **Status**: Running (serves static files from wwwroot)

### Known Issues & Limitations
1. **.NET Development Server Issues**: The standard `dotnet run` command times out in current Replit environment
2. **Missing Build Artifacts**: Blazor WebAssembly framework files (`_framework/blazor.webassembly.js`) not generated
3. **Incomplete Functionality**: Application loads basic HTML but lacks full Blazor interactivity

### Deployment Configuration
- **Target**: Autoscale (stateless web application)
- **Build**: `dotnet publish -c Release -o ./publish`
- **Run**: Python HTTP server serving from `./publish/wwwroot`

## Development Workflow

### To Complete Full Setup (Future Tasks)
1. **Resolve .NET Build Issues**: Investigate why `dotnet restore` and `dotnet build` hang
2. **Generate Framework Files**: Successfully build Blazor WebAssembly to create `_framework` directory
3. **Test Full Application**: Verify all Blazor components and routing work properly
4. **Backend Integration**: Set up and connect microservices if needed

### File Structure
```
/NexaCRMSolution
├── src/
│   ├── ApiGateway/           # API Gateway service
│   ├── BuildingBlocks/       # Shared libraries
│   ├── Services/             # Microservices
│   │   ├── Contact.API/
│   │   ├── Deal.API/
│   │   └── Identity.API/
│   └── Web/
│       └── NexaCRM.WebClient/ # Blazor WebAssembly frontend
├── tests/                    # Unit tests
├── supabase/                # Database migrations
└── NexaCrmSolution.sln      # Solution file
```

## User Preferences
- **Language**: Multi-language support (Korean primary, English secondary)
- **Deployment**: Cloud-native, microservices architecture
- **Frontend**: Modern, responsive web application
- **Development**: .NET 8 ecosystem with C#

## Next Steps
1. Debug .NET build process in Replit environment
2. Complete Blazor WebAssembly build pipeline
3. Set up database connections (PostgreSQL/Supabase)
4. Configure microservices communication
5. Test end-to-end application functionality

## Technical Notes
- Uses Bootstrap 5.3.0 for UI components
- Tailwind CSS for additional styling
- Multiple resource files for internationalization
- Mock services implemented for development/testing
- Authentication state management configured