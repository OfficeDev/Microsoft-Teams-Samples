# GitHub Copilot Agent Instructions - Microsoft Teams Bot Conversation Sample (Node.js)

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview
This sample demonstrates a Microsoft Teams bot built with Node.js and Bot Framework SDK v4. It showcases advanced conversational flow, Teams-specific API interactions, proactive messaging, adaptive cards, AI-formatted messages, and comprehensive conversation event handling within Microsoft Teams.

**Key Features:**
- Bot conversation events (personal, group chat, team scope)
- Adaptive Cards with immersive reader support
- AI-formatted messages with citations, feedback buttons, and sensitivity labels
- Proactive messaging to team members
- Read receipt tracking
- Message update events (edit/delete/restore)
- Teams-specific calls and integrations

## Architecture & File Structure
```
/
├── index.js                    # Main server setup and bot initialization
├── bots/
│   └── teamsConversationBot.js # Core bot logic and Teams event handlers
├── resources/                  # Adaptive card templates and JSON resources
├── appManifest/               # Teams app manifest configuration
├── deploymentTemplates/       # Azure deployment templates
├── .env                       # Environment configuration
├── package.json               # Dependencies and scripts
└── README.md                  # Setup and usage documentation
```

## Coding Guidelines

### JavaScript Standards
- Use **ES6+ syntax** (const/let, arrow functions, async/await)
- Prefer `async/await` over Promises for asynchronous operations
- Use destructuring for cleaner code: `const { TeamsActivityHandler, CardFactory } = require('botbuilder');`
- Implement proper error handling with try-catch blocks

### Bot Framework Patterns
- Extend `TeamsActivityHandler` for Teams-specific functionality
- Use `CloudAdapter` with `ConfigurationBotFrameworkAuthentication` for modern bot setup
- Implement conversation state management with `ConversationState` and `MemoryStorage`
- Handle bot lifecycle events: `onMembersAdded`, `onMessage`, `onMessageUpdate`, `onMessageDelete`

### Teams Integration Best Practices
- Use `TeamsInfo` for Teams-specific operations (getting members, channels, etc.)
- Implement proactive messaging with conversation references
- Handle Teams-specific events: `onTeamsMembersAdded`, `onTeamsChannelCreated`
- Use adaptive cards with `CardFactory.adaptiveCard()` for rich UI

### Code Organization
- Separate concerns: main server logic in `index.js`, bot logic in dedicated bot classes
- Use modular imports: group related Bot Framework imports together
- Create reusable card templates in separate JSON files
- Implement helper classes for complex operations (GraphHelper, etc.)

## Environment Configuration
```env
MicrosoftAppId=
MicrosoftAppPassword=
MicrosoftAppTenantId=
MicrosoftAppType=MultiTenant
BaseUrl=https://your-ngrok-url.ngrok.io
```

## Core Dependencies & Imports
```javascript
// Essential Bot Framework imports
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication,
    TeamsActivityHandler,
    CardFactory,
    MessageFactory,
    TeamsInfo,
    ActionTypes,
    ActivityTypes
} = require('botbuilder');

// Server setup
const restify = require('restify');
const path = require('path');
require('dotenv').config({ path: path.join(__dirname, '.env') });
```

## Bot Implementation Patterns

### Main Bot Class Structure
```javascript
class TeamsConversationBot extends TeamsActivityHandler {
    constructor() {
        super();
        
        // Handle new members added
        this.onMembersAdded(async (context, next) => {
            // Send welcome message
            await next();
        });
        
        // Handle messages
        this.onMessage(async (context, next) => {
            // Process message logic
            await next();
        });
        
        // Handle Teams-specific events
        this.onTeamsMembersAdded(async (context, next) => {
            // Teams member addition logic
            await next();
        });
    }
}
```

### Server Setup Pattern
```javascript
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Error handling
adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);
    await context.sendTraceActivity('OnTurnError Trace', `${error}`, 'https://www.botframework.com/schemas/error', 'TurnError');
};

// Create bot instance
const bot = new TeamsConversationBot();

// Setup server
const server = restify.createServer();
server.use(restify.plugins.bodyParser());
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (turnContext) => {
        await bot.run(turnContext);
    });
});
```

## Advanced Features Implementation

### Adaptive Cards with AI Features
- Use citations for source references
- Implement feedback buttons for user engagement
- Add sensitivity labels for content classification
- Support immersive reader for accessibility

### Proactive Messaging
- Store conversation references for later use
- Implement message broadcasting to team members
- Handle read receipt tracking

### Message Event Handling
- Process message updates (edited messages)
- Handle soft-deleted messages
- Manage message restoration events

## Testing & Debugging

### Development Setup
- Use `npm start` to run the bot locally
- Use ngrok or dev tunnels for local testing: `ngrok http 3978`
- Enable debug mode by uncommenting debug lines in error handlers
- Test with Bot Framework Emulator for quick validation

### Unit Testing Guidelines
- Write tests for all message handlers using Jest or Mocha
- Mock Bot Framework context and adapter
- Test adaptive card generation and validation
- Validate Teams-specific API interactions

### Debugging Tips
- Uncomment debug lines in error handlers for detailed error info
- Use `context.sendTraceActivity()` for tracking bot flow
- Log conversation references for proactive messaging debugging
- Validate manifest.json before uploading to Teams

## Teams App Manifest Configuration
- Set valid domains for bot endpoints
- Configure supported scopes: personal, team, groupChat
- Include bot commands in manifest for discoverability
- Update app icons and descriptions appropriately

## Deployment Guidelines

### Azure Bot Service Setup
- Create Azure Bot resource with "Use existing app registration"
- Configure messaging endpoint: `https://your-domain.com/api/messages`
- Enable Teams channel in bot configuration
- Set environment variables in Azure App Service

### Security Best Practices
- Store secrets in environment variables, never in code
- Use Azure Key Vault for production secrets
- Validate incoming requests from Bot Framework
- Implement proper CORS configuration

## Common Development Tasks
- **Add new commands**: Extend message handler with command parsing
- **Create adaptive cards**: Use JSON templates with data binding
- **Implement proactive messaging**: Store and use conversation references
- **Add Teams API calls**: Use TeamsInfo for Teams-specific operations
- **Handle file uploads**: Process attachment activities
- **Integrate with Graph API**: Add authentication for enhanced features

## Documentation Standards
- Use JSDoc comments for all public methods
- Document complex card templates and their data structures
- Maintain README.md with current setup instructions
- Include example usage for all bot commands

## Performance Considerations
- Use conversation state efficiently, avoid storing large objects
- Implement pagination for member lists and data queries
- Cache frequently accessed data appropriately
- Monitor bot response times and optimize accordingly

## Additional Resources
- [Bot Framework Documentation](https://docs.botframework.com/)
- [Teams Bot Development Guide](https://docs.microsoft.com/microsoftteams/platform/bots/)
- [Adaptive Cards Schema](https://adaptivecards.io/explorer/)
- [Teams JavaScript SDK](https://docs.microsoft.com/javascript/api/@microsoft/teams-js/)

## Error Handling Patterns
```javascript
// Standard error handling in bot methods
try {
    // Bot operation
    await context.sendActivity(MessageFactory.text('Response'));
} catch (error) {
    console.error('Bot error:', error);
    await context.sendActivity(MessageFactory.text('Sorry, something went wrong.'));
}
```

Remember to follow the existing code patterns and conventions when extending the bot functionality.