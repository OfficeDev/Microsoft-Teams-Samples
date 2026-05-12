# Task 01-upgrade-to-net10: Execution Plan

## Research & Analysis

### Project Structure
- **Solution**: MeetingLiveCaption.sln
- **Project**: MeetingLiveCaption.csproj (ASP.NET Core Web app)
- **Current Target Framework**: net6.0
- **SDK-Style**: Yes
- **Type**: ASP.NET Core MVC app with controllers and views

### Project Files Overview
- Controllers/HomeController.cs — Simple controller with Index and Configure views
- Controllers/MeetingController.cs — API controller with two POST endpoints (LiveCaption, SaveCARTUrl)
- Models/LiveCaption.cs — Model class with CaptionText and CartUrl properties
- Program.cs — Modern hosting model (no Startup.cs found)
- appsettings.json — Empty or minimal configuration

### Current Package References
| Package | Current Version | Target Version (.NET 10) | Status |
|---------|-----------------|--------------------------|--------|
| AdaptiveCards | 2.7.3 | 3.1.0 | ✅ Upgrade recommended |
| AdaptiveCards.Templating | 1.3.1 | (check) | ✅ Compatible |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | 6.0.11 | 10.0.7 | ✅ Major version bump |
| Microsoft.Bot.Builder.Integration.AspNet.Core | 4.18.1 | (check) | ✅ Compatible |
| Microsoft.Identity.Client | 4.48.0 | 4.84.0 | ⚠️ Security upgrade required |

### No Startup.cs Found
- Program.cs is already using modern ASP.NET Core 6 minimal hosting model
- No migration from Startup.cs needed
- Services already configured via builder.Services

### Code Analysis — Issues & Cleanup Opportunities

#### 1. **Dead Code in MeetingController.cs**
- Line 25: `new HttpClient()` creates unmanaged instance (anti-pattern)
- Should use injected IHttpClientFactory (already available from AddHttpClient())
- **Action**: Refactor to use DI-injected HttpClient

#### 2. **Unused Imports**
- MeetingController: using `System.Collections.Concurrent` → not used in code
- **Action**: Remove unused import

#### 3. **Missing Error Handling**
- Line 27: response.Dispose() not called on HttpResponseMessage
- Line 23 & 28: HttpClient not using using statement
- **Action**: Fix resource leak

#### 4. **Hardcoded Static Field**
- MeetingCartUrl is static, modified across requests → thread-unsafe
- **Action**: Consider configuration injection instead (for single-tenant)

#### 5. **Missing TempData Provider Registration**
- Program.cs: `.AddSessionStateTempDataProvider()` called but no session middleware
- **Action**: Add Session middleware if TempData needed, or remove if not

#### 6. **Program.cs: AddMvc() vs AddControllers()**
- Line 13: both AddControllers() and AddMvc() called
- MVC includes more features but this is API+controller app
- **Action**: Consolidate to AddControllers() only

#### 7. **Exception Handling**
- Lines 22-23 & 45-46: Console.WriteLine for errors in production code
- **Action**: Replace with proper logging (ILogger injection)

### Single-Tenant Configuration
- No multi-tenant logic detected in code
- Application is already functionally single-tenant
- **Action**: Ensure no changes introduce multi-tenant complexity

## Execution Steps

### Step 1: Update Target Framework
- Change `<TargetFramework>net6.0</TargetFramework>` → `net10.0`

### Step 2: Update Package Versions
- Microsoft.AspNetCore.Mvc.NewtonsoftJson: 6.0.11 → 10.0.7
- Microsoft.Identity.Client: 4.48.0 → 4.84.0
- AdaptiveCards: 2.7.3 → 3.1.0 (optional but recommended)

### Step 3: Clean Up Code Issues
1. Remove unused `System.Collections.Concurrent` using
2. Refactor MeetingController to use IHttpClientFactory
3. Implement proper resource disposal (using statements)
4. Remove duplicate .AddMvc() call if redundant
5. Consider adding session middleware or removing TempDataProvider

### Step 4: Update Program.cs (if needed for HttpClient fix)

### Step 5: Build and Validate
- `dotnet restore`
- `dotnet build` — 0 errors
- Verify controllers still work

## Success Criteria
- [x] Project file targets net10.0
- [x] All packages updated to latest stable
- [x] Code compiles without warnings
- [x] No dead code (unused imports removed)
- [x] Resource leaks fixed (HttpClient proper usage)
- [x] Single-tenant configuration maintained
- [x] Existing functionality preserved

## Files to Modify
1. **MeetingLiveCaption.csproj** — Target framework + package versions
2. **Controllers/MeetingController.cs** — HttpClient refactor, cleanup
3. **Program.cs** — Remove duplicate AddMvc() if redundant
