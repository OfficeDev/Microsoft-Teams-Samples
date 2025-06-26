# Copilot Agent Configuration for Microsoft Teams Bot

<!--
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
-->

## 🎯 Project Overview
This Microsoft Teams Hello World Bot sample is optimized for GitHub Copilot Agent development, providing comprehensive examples of:
- Teams bot development patterns
- Messaging extension implementation
- Tab configuration and routing
- Error handling and state management
- Modern JavaScript/Node.js best practices

## 🤖 Copilot Integration Points

### Code Generation Assistance
- **Bot Message Handlers**: Use existing patterns in `src/bot/teams-echo-bot.js`
- **Messaging Extensions**: Reference `src/messaging-extensions/teams-message-extension.js`
- **Tab Routing**: Follow patterns in `src/tabs/tab-router.js`
- **Error Handling**: Implement consistent error patterns throughout

### Development Workflow
1. **Initialization**: Copy and modify existing bot handlers
2. **Extension**: Use JSDoc patterns for new functionality
3. **Testing**: Follow error handling and validation patterns
4. **Deployment**: Use configuration management patterns

### Key Patterns to Follow
- Comprehensive JSDoc documentation on all public methods
- Async/await for all asynchronous operations
- Proper error boundaries with user-friendly messages
- Modular file organization with clear separation of concerns
- Environment-based configuration management

## 📚 Documentation Standards
- All classes must extend appropriate base classes (TeamsActivityHandler)
- All public methods require JSDoc with @param, @returns, and @example
- Error handling must include logging and user feedback
- Configuration should be externalized and validated

## 🔧 Development Commands
```bash
npm run dev          # Start development server with hot reload
npm run start:prod   # Start production server
npm run lint         # Run ESLint for code quality
npm run test:unit    # Run unit tests
npm run validate     # Run full validation suite
```

## 🎪 Common Use Cases
- Creating new bot command handlers
- Adding messaging extension commands
- Implementing custom tab functionality
- Setting up bot authentication
- Adding external service integrations
- Implementing conversation state management

This configuration ensures Copilot can effectively assist with Teams bot development while maintaining code quality and consistency.
