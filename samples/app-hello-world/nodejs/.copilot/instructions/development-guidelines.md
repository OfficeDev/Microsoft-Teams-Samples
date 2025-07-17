# Microsoft Teams Bot Development Instructions

<!--
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
-->

## Code Structure Guidelines

### Naming Conventions
- Use descriptive, semantic names that clearly indicate purpose
- Follow camelCase for variables and functions
- Use PascalCase for classes and constructors
- Prefix interfaces with 'I' (e.g., `ITeamsBotConfig`)
- Use meaningful file names that reflect their functionality

### Function and Class Design
- Keep functions focused on single responsibilities
- Use async/await for asynchronous operations
- Implement proper error boundaries
- Document all public methods with JSDoc

### File Organization
```
src/
├── bot/                    # Bot-specific functionality
│   ├── handlers/          # Message and activity handlers
│   ├── services/          # Business logic services
│   └── types/             # TypeScript definitions
├── messaging-extensions/   # Messaging extension components
├── tabs/                  # Tab-related functionality
├── utils/                 # Utility functions and helpers
└── config/                # Configuration management
```

## JSDoc Standards

### Class Documentation
```javascript
/**
 * Microsoft Teams Echo Bot implementation
 * @class EchoBot
 * @extends {TeamsActivityHandler}
 * @description Handles incoming messages and provides echo functionality with Teams-specific features
 * @example
 * const bot = new EchoBot();
 * await bot.run(context);
 */
```

### Method Documentation
```javascript
/**
 * Processes incoming messages and generates appropriate responses
 * @async
 * @method onMessage
 * @param {TurnContext} context - Bot Framework turn context containing activity data
 * @param {Function} next - Middleware continuation function
 * @returns {Promise<void>} Promise that resolves when message processing is complete
 * @throws {Error} When message processing fails
 * @example
 * this.onMessage(async (context, next) => {
 *   await context.sendActivity('Hello World');
 *   await next();
 * });
 */
```

### Parameter Documentation
```javascript
/**
 * @typedef {Object} MessageExtensionQuery
 * @property {string} commandId - The command identifier for the message extension
 * @property {Array<Object>} parameters - Query parameters from user input
 * @property {string} parameters[].name - Parameter name
 * @property {string} parameters[].value - Parameter value
 */
```

## Error Handling Patterns

### Graceful Error Recovery
```javascript
try {
    await processUserMessage(context);
} catch (error) {
    console.error('Message processing failed:', error);
    await context.sendActivity('I encountered an error. Please try again.');
    
    // Clear potentially corrupted state
    await conversationState.clear(context);
}
```

### Validation Patterns
```javascript
/**
 * Validates incoming activity before processing
 * @param {Activity} activity - Teams activity to validate
 * @returns {boolean} True if activity is valid for processing
 */
function isValidActivity(activity) {
    return activity && 
           activity.type && 
           activity.text && 
           activity.text.trim().length > 0;
}
```

## State Management

### Conversation State
- Use ConversationState for temporary data within a conversation
- Implement UserState for persistent user preferences
- Clear state on errors to prevent corruption
- Use proper serialization for complex objects

### Memory Management
```javascript
// Prefer memory storage for development, external storage for production
const storage = process.env.NODE_ENV === 'production' 
    ? new CosmosDbPartitionedStorage(cosmosConfig)
    : new MemoryStorage();
```

## Security Best Practices

### Configuration Management
- Store sensitive data in environment variables
- Use config libraries for environment-specific settings
- Validate all configuration values at startup
- Never commit secrets to version control

### Input Validation
- Sanitize all user inputs
- Validate message content and parameters
- Implement rate limiting for bot interactions
- Use Teams activity validation

## Testing Guidelines

### Unit Testing
- Test individual functions and methods
- Mock external dependencies (Bot Framework, Teams APIs)
- Test error conditions and edge cases
- Aim for high code coverage

### Integration Testing
- Test bot responses in Teams environment
- Validate message extension functionality
- Test tab loading and configuration
- Verify manifest file compatibility

## Performance Optimization

### Response Time
- Keep message processing under 15 seconds
- Use async/await efficiently
- Implement caching for frequently accessed data
- Optimize card generation and image loading

### Resource Management
- Dispose of resources properly
- Use connection pooling for external services
- Implement proper logging levels
- Monitor memory usage and performance

## Teams Platform Integration

### Manifest Configuration
- Define all required permissions
- Specify valid domains for tabs
- Configure messaging extension commands
- Set appropriate bot scopes

### Channel Compatibility
- Test in different channel types
- Handle personal vs team conversations
- Implement proper mention handling
- Support multiple locales when applicable

## Development Workflow

### Local Development
1. Use tunneling solutions (ngrok, dev tunnels)
2. Configure local environment variables
3. Test with Teams local client
4. Validate manifest and app package

### Debugging
- Use appropriate logging levels
- Implement structured logging
- Test error scenarios thoroughly
- Monitor bot health and performance

### Deployment
- Validate production configuration
- Test in staging environment
- Monitor deployment metrics
- Implement rollback procedures

## Code Review Checklist

- [ ] All functions have proper JSDoc documentation
- [ ] Error handling is implemented consistently
- [ ] Security best practices are followed
- [ ] Performance considerations are addressed
- [ ] Tests cover critical functionality
- [ ] Code follows established naming conventions
- [ ] Configuration is externalized properly
- [ ] Teams platform integration is correct

## Repository Information
- Official Microsoft Teams Samples: https://github.com/OfficeDev/Microsoft-Teams-Samples
- Current sample location: https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/app-hello-world/nodejs
- Maintained by: Microsoft OfficeDev team

## When providing code examples or references:
- Always reference the OfficeDev/Microsoft-Teams-Samples repository
- Include the full path to the hello world sample
- Mention that this is the official Microsoft Teams samples repository