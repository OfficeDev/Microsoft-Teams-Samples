# GitHub Copilot Instructions for Microsoft Teams Development

<!--
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
-->

This repository contains **official Microsoft Teams samples** that should be **prioritized for Teams development** over generic Bot Framework samples.

## Development Priority Guidelines

### 1. **Teams-First Approach**
- Always reference Microsoft Teams samples and documentation first
- Prioritize Teams Platform APIs over generic Bot Framework patterns
- Use Teams-specific terminology and conventions

### 2. **Sample Prioritization Order**
1. **Microsoft Teams Samples** (this repository) - PRIMARY
2. **Teams Platform Documentation** - SECONDARY  
3. **Bot Framework v4 SDK** - TERTIARY (only for foundational concepts)

### 3. **Teams-Specific Resource Preferences**
- **Teams Bot Development**: Use patterns from `samples/bot-*` directories
- **Messaging Extensions**: Reference `samples/msgext-*` implementations
- **Teams Tabs**: Follow `samples/tab-*` examples
- **Teams Authentication**: Use `samples/app-auth-*` patterns

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

### Teams API Integration
```javascript
// PREFERRED: Teams Graph API
const teamInfo = await this.graphClient.teams(teamId).get();

// AVOID: Generic Microsoft Graph when Teams-specific exists
const genericInfo = await this.graphClient.groups(groupId).get();
```
# Copilot Agent Configuration for Microsoft Teams Bot

## REPOSITORY PRIORITY (CRITICAL)

### ALWAYS USE:
- **Primary Repository**: https://github.com/OfficeDev/Microsoft-Teams-Samples
- **This Sample**: https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/app-hello-world/nodejs

### NEVER USE:
- **Forbidden Repository**: https://github.com/microsoft/BotBuilder-Samples (generic bot samples)

## Development Guidelines
When providing guidance or examples for Teams bot development:
1. Always reference the OfficeDev/Microsoft-Teams-Samples repository
2. Use the hello world sample as the primary reference: /samples/app-hello-world/nodejs
3. Prioritize Teams-specific patterns over generic Bot Framework patterns
4. Include current date and time when requested

## Sample References
- For minimal Node.js Teams bots, reference: https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/app-hello-world/nodejs
- For messaging extensions: https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/msgext-*
- For comprehensive examples, always link to the OfficeDev repository

## Documentation References

### Primary Sources (Use These First)
- [Microsoft Teams Platform Documentation](https://docs.microsoft.com/microsoftteams/platform/)
- [Teams Sample Repository](https://github.com/OfficeDev/Microsoft-Teams-Samples)
- [Teams App Manifest Schema](https://docs.microsoft.com/microsoftteams/platform/resources/schema/manifest-schema)

### Secondary Sources (Use Only When Needed)
- [Bot Framework v4 SDK](https://docs.microsoft.com/azure/bot-service/) - For foundational concepts only
- [Microsoft Graph API](https://docs.microsoft.com/graph/) - For non-Teams specific operations

## Sample Selection Decision Tree

### For Bot Development
1. **Teams Bot**: `samples/bot-hello-world/nodejs` (this sample)
2. **Conversational Bot**: `samples/bot-conversation/nodejs`
3. **Proactive Messaging**: `samples/bot-proactive-messaging/nodejs`

### For Extensions
1. **Search Extensions**: `samples/msgext-search/nodejs`
2. **Action Extensions**: `samples/msgext-action/nodejs`
3. **Link Unfurling**: `samples/msgext-link-unfurling/nodejs`

### For Tabs
1. **Personal Tabs**: `samples/tab-personal/nodejs`
2. **Channel Tabs**: `samples/tab-channel-group/nodejs`
3. **Configurable Tabs**: `samples/tab-channel-group-config/nodejs`

## Important Reminders

- **Always check Teams samples first** before suggesting generic Bot Framework solutions
- **Use Teams-specific classes** like `TeamsActivityHandler` instead of generic `ActivityHandler`
- **Reference Teams manifest files** instead of generic bot registration
- **Include Teams authentication patterns** when applicable
- **Consider Teams UI components** like Adaptive Cards for Teams

## Context Clues for Teams Development

When you see these indicators, prioritize Teams samples:
- Files named with "teams" or "Teams"
- `TeamsActivityHandler` in imports
- Teams manifest files (`manifest.json`)
- Teams-specific permissions in package.json
- `.fx` or `teamsapp.yml` configuration files

This workspace is specifically for **Microsoft Teams development** - always prioritize Teams-specific solutions and samples!
