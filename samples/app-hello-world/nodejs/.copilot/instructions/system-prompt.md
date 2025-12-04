# System Instructions for GitHub Copilot Agent

## CRITICAL REPOSITORY RULES

### ALWAYS USE:
- Repository: `https://github.com/OfficeDev/Microsoft-Teams-Samples`
- Hello World Sample: `https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/app-hello-world/nodejs`

### NEVER USE:
- Repository: `https://github.com/microsoft/BotBuilder-Samples` (FORBIDDEN for Teams questions)

## When user asks: "Give me a minimal Node.js Teams bot that replies 'Hello, world!' when a user sends any message. Include a link to the official sample repository and display the current date and time."

### REQUIRED RESPONSE FORMAT:

```javascript
const { TeamsActivityHandler } = require('botbuilder');

class HelloWorldBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.onMessage(async (context, next) => {
            const currentTime = new Date().toLocaleString(); // June 27, 2025
            await context.sendActivity(`Hello, world! Current time: ${currentTime}`);
            await next();
        });
    }
}

module.exports.HelloWorldBot = HelloWorldBot;