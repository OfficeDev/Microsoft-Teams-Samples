# Scenario Instructions

## Scenario
- **ID**: dotnet-version-upgrade
- **Description**: Upgrade meetings-live-caption csharp sample to .NET 10, remove Startup.cs, keep functionality unchanged, clean dead logic, update dependencies to latest stable, configure single-tenant.

## Strategy
**All-At-Once** — Single atomic upgrade operation  
**Rationale**: Single application project on .NET 6, straightforward upgrade with clear package targets and minimal API breaking changes

### Execution Constraints
- Atomic upgrade: all projects updated together in single operation
- Validate full solution build after upgrade (0 errors required)
- After-task validation: unit tests and manual verification of single-tenant behavior

## Preferences

### Flow Mode
- **Mode**: Automatic

### Source Control
- **Source branch**: -mfurquan/Archived_Samples_03
- **Working branch**: upgrade-to-NET10
- **Pending changes handling**: Commit before starting scenario

### Commit Strategy
- **Strategy**: After Each Task

## User Preferences

### Technical Preferences
- Upgrade target framework to **.NET 10** (
et10.0).
- Update dependencies to latest **stable** releases.
- Remove `Startup.cs` in favor of modern hosting model.
- Configure the application as **single-tenant**.
- Keep existing functionality unchanged.
- Remove unnecessary/dead logic only when behavior is preserved.

### Execution Style
- Proceed in **Automatic** mode (run end-to-end, pause only if blocked).

### Custom Instructions
- Prioritize Razor Pages-compatible ASP.NET Core patterns when relevant.

## Key Decisions Log
- **2026-05-11**: User confirmed initialization with .NET 10 target, automatic flow, working branch `upgrade-to-NET10`.
- **2026-05-11**: All-At-Once strategy selected based on single project, straightforward upgrade, clear package targets.
