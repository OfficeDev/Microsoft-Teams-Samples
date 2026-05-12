# .NET 10 Upgrade Plan

## Overview

**Target**: Upgrade meetings-live-caption C# sample from .NET 6 to .NET 10  
**Scope**: 1 application project (MeetingLiveCaption.csproj), modern ASP.NET Core, SDK-style format  
**Strategy**: All-At-Once — single atomic upgrade operation

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single operation.  
**Rationale**: Single application project on .NET 6, straightforward upgrade path to .NET 10 with clear package update targets and minimal API breaking changes.

## Tasks

### 01-upgrade-to-net10

Upgrade the meetings-live-caption application from .NET 6 to .NET 10, including target framework update, package version bumps, dependency resolution, and code compatibility fixes.

**Includes**:
- Update MeetingLiveCaption.csproj to target 
et10.0
- Update NuGet packages to latest stable versions:
  - Microsoft.AspNetCore.Mvc.NewtonsoftJson: 6.0.11 → 10.0.7
  - Microsoft.Identity.Client: 4.48.0 → 4.84.0
- Remove Startup.cs and consolidate configuration into Program.cs (modern ASP.NET Core hosting model)
- Update appsettings.json and configuration injection patterns as needed
- Configure the application as single-tenant
- Clean up any dead/unnecessary logic
- Fix any breaking changes from the HttpClientFactory behavioral change
- Ensure solution builds without errors

**Done when**: 
- MeetingLiveCaption.csproj targets net10.0
- All packages updated to latest stable versions
- Startup.cs removed
- Solution builds successfully with 0 errors
- Application is configured as single-tenant
- All existing functionality preserved and working
