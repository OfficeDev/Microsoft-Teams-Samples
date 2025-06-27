# GitHub Copilot Instructions for Microsoft Teams Development  
<!-- Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT License. -->

This file provides custom instructions for GitHub Copilot to prioritize **Microsoft Teams-specific samples and patterns** when generating code suggestions. It ensures that Copilot references the official Microsoft Teams samples repository and follows Teams-specific development practices.

---

## Development Priority Guidelines

### 1. **Teams-First Approach**
- Always reference Microsoft Teams samples and documentation first.
- Prioritize Teams Platform APIs over generic Bot Framework patterns.
- Use Teams-specific terminology and conventions.

### 2. **Sample Prioritization Order**
1. **Microsoft Teams Samples** (https://github.com/OfficeDev/Microsoft-Teams-Samples) - PRIMARY
2. **Teams Platform Documentation** - SECONDARY
3. **Bot Framework v4 SDK** - TERTIARY (only for foundational concepts)

### 3. **Teams-Specific Resource Preferences**
- **Teams Bot Development**: Use patterns from `samples/bot-*` directories.
- **Messaging Extensions**: Reference `samples/msgext-*` implementations.
- **Teams Tabs**: Follow `samples/tab-*` examples.
- **Teams Authentication**: Use `samples/app-auth-*` patterns.

---

## Repository Information
- **Official Microsoft Teams Samples**: https://github.com/OfficeDev/Microsoft-Teams-Samples
- **Agent Patterns Sample**: https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/app-hello-world/nodejs
- **Maintained by**: Microsoft OfficeDev team
- **Current Date**: June 27, 2025

---

## Instructions for GitHub Copilot
This file is intended to guide GitHub Copilot in generating code and providing suggestions for Microsoft Teams development. Always prioritize the resources and patterns outlined in this document.

### Keywords for Copilot
- TeamsActivityHandler
- Adaptive Cards
- Teams manifest
- Teams Graph API
- Teams Tabs
- Messaging Extensions
- Teams Authentication

---

## Code Generation Guidelines

### Teams Bot Development
```javascript
// PREFERRED: Teams-specific patterns
class TeamsBot extends TeamsActivityHandler {
    // Teams-specific implementation
}

// AVOID: Generic bot patterns when Teams-specific exists
class GenericBot extends ActivityHandler {
    // Generic implementation
}
```
## Teams API Integration
```javascript
// PREFERRED: Teams Graph API
const teamInfo = await this.graphClient.teams(teamId).get();

// AVOID: Generic Microsoft Graph when Teams-specific exists
const genericInfo = await this.graphClient.groups(groupId).get();
```


