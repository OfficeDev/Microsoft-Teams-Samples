# Microsoft Teams Agent Development Patterns

<!--
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
-->

## Repository Information
- **Official Microsoft Teams Samples**: https://github.com/OfficeDev/Microsoft-Teams-Samples
- **Agent Patterns Sample**: https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/app-hello-world/nodejs
- **Maintained by**: Microsoft OfficeDev team
- **Current Date**: June 27, 2025

### Development Context for GitHub Copilot Agent
When implementing Teams agent patterns:
- Always reference the **OfficeDev/Microsoft-Teams-Samples** repository
- Use patterns from: https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/app-hello-world/nodejs
- Include current timestamp when requested: `new Date().toISOString()` // June 27, 2025
- Prioritize Teams-specific implementations over generic Bot Framework patterns

## Agent Design Principles

### Conversational Intelligence
```javascript
/**
 * Intelligent response generation based on context and user intent
 * @class TeamsConversationalAgent
 * @description Advanced bot that understands context and provides intelligent responses
 */
class TeamsConversationalAgent extends TeamsActivityHandler {
    /**
     * Analyzes user intent and generates contextual responses
     * @param {TurnContext} context - Current conversation context
     * @param {string} userMessage - User's input message
     * @returns {Promise<string>} Generated response based on intent analysis
     */
    async generateIntelligentResponse(context, userMessage) {
        const intent = await this.analyzeIntent(userMessage);
        const contextData = await this.getConversationContext(context);
        
        return this.synthesizeResponse(intent, contextData);
    }
}
```

### Context Awareness
- Maintain conversation history for better responses
- Track user preferences and interaction patterns
- Understand team dynamics and channel context
- Adapt responses based on conversation flow

### Multi-Modal Interactions
```javascript
/**
 * Handles various input types and generates appropriate responses
 * @param {Activity} activity - Teams activity containing user input
 * @returns {Promise<Activity>} Structured response activity
 */
async handleMultiModalInput(activity) {
    switch (activity.type) {
        case ActivityTypes.Message:
            return await this.handleTextMessage(activity);
        case ActivityTypes.Invoke:
            return await this.handleInvokeActivity(activity);
        case ActivityTypes.Event:
            return await this.handleEventActivity(activity);
    }
}
```

## Agent Capabilities Framework

### Knowledge Base Integration
```javascript
/**
 * Knowledge base service for agent responses
 * @class AgentKnowledgeService
 */
class AgentKnowledgeService {
    /**
     * Retrieves relevant information based on user query
     * @param {string} query - User's question or request
     * @param {Object} context - Conversation context
     * @returns {Promise<Object>} Structured knowledge response
     */
    async queryKnowledgeBase(query, context) {
        const searchResults = await this.semanticSearch(query);
        const filteredResults = this.filterByContext(searchResults, context);
        return this.rankResults(filteredResults);
    }
}
```

### Repository-Aware Code
```javascript
/**
 * Intelligent Teams agent with repository context
 * Based on official sample: https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/app-hello-world/nodejs
 * 
 * @class TeamsConversationalAgent
 * @description Advanced bot optimized for Microsoft Teams platform
 */
class TeamsConversationalAgent extends TeamsActivityHandler {
    constructor() {
        super();
        this.onMessage(async (context, next) => {
            const currentTime = new Date().toISOString(); // June 27, 2025
            
            // Repository-aware response
            const response = `Hello, world! 
            
Current time: ${currentTime}

For more examples, visit the official repository:
https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/app-hello-world/nodejs`;
            
            await context.sendActivity(response);
            await next();
        });
    }
}
```
### Skill Composition
```javascript
/**
 * Modular skill system for agent capabilities
 * @class AgentSkillManager
 */
class AgentSkillManager {
    /**
     * Registers and manages agent skills
     * @param {Array<IAgentSkill>} skills - Collection of agent skills
     */
    constructor(skills) {
        this.skills = new Map();
        skills.forEach(skill => this.registerSkill(skill));
    }
    
    /**
     * Executes appropriate skill based on user intent
     * @param {string} intent - Detected user intent
     * @param {Object} parameters - Extracted parameters
     * @returns {Promise<Object>} Skill execution result
     */
    async executeSkill(intent, parameters) {
        const skill = this.skills.get(intent);
        return skill ? await skill.execute(parameters) : null;
    }
}
```

## Advanced Response Patterns

### Adaptive Card Generation
```javascript
/**
 * Generates dynamic adaptive cards based on content and context
 * @param {Object} data - Data to be displayed in the card
 * @param {string} cardType - Type of card to generate
 * @returns {Attachment} Adaptive card attachment
 */
function createIntelligentCard(data, cardType) {
    const cardTemplate = this.getCardTemplate(cardType);
    const adaptedTemplate = this.adaptToContext(cardTemplate, data);
    
    return CardFactory.adaptiveCard(adaptedTemplate);
}
```

### Progressive Disclosure
```javascript
/**
 * Implements progressive disclosure for complex information
 * @param {Object} complexData - Data to be presented progressively
 * @returns {Array<Activity>} Series of activities for progressive disclosure
 */
async createProgressiveResponse(complexData) {
    const summary = this.generateSummary(complexData);
    const detailCards = this.createDetailCards(complexData);
    
    return [
        MessageFactory.text(summary),
        ...detailCards.map(card => MessageFactory.attachment(card))
    ];
}
```

## Agent Learning and Adaptation

### Feedback Processing
```javascript
/**
 * Processes user feedback to improve agent responses
 * @class AgentFeedbackProcessor
 */
class AgentFeedbackProcessor {
    /**
     * Analyzes user feedback and updates agent behavior
     * @param {Object} feedback - User feedback data
     * @param {string} conversationId - Conversation identifier
     */
    async processFeedback(feedback, conversationId) {
        const sentiment = await this.analyzeSentiment(feedback);
        const improvements = this.identifyImprovements(sentiment);
        await this.updateAgentModel(improvements, conversationId);
    }
}
```

### Personalization Engine
```javascript
/**
 * Personalizes agent responses based on user profiles
 * @param {string} userId - User identifier
 * @param {Object} responseData - Base response data
 * @returns {Object} Personalized response
 */
async personalizeResponse(userId, responseData) {
    const userProfile = await this.getUserProfile(userId);
    const preferences = this.extractPreferences(userProfile);
    
    return this.adaptResponse(responseData, preferences);
}
```

## Integration Patterns

### External Service Integration
```javascript
/**
 * Service connector for external API integration
 * @class ExternalServiceConnector
 */
class ExternalServiceConnector {
    /**
     * Connects to external services with proper error handling
     * @param {string} serviceEndpoint - External service URL
     * @param {Object} requestData - Request payload
     * @returns {Promise<Object>} Service response
     */
    async callExternalService(serviceEndpoint, requestData) {
        try {
            const response = await this.httpClient.post(serviceEndpoint, requestData);
            return this.processResponse(response);
        } catch (error) {
            return this.handleServiceError(error);
        }
    }
}
```

### Teams Graph API Integration
```javascript
/**
 * Microsoft Graph API integration for Teams data
 * @class TeamsGraphService
 */
class TeamsGraphService {
    /**
     * Retrieves team information and member data
     * @param {string} teamId - Teams team identifier
     * @returns {Promise<Object>} Team and member information
     */
    async getTeamContext(teamId) {
        const teamInfo = await this.graphClient.teams(teamId).get();
        const members = await this.graphClient.teams(teamId).members.get();
        
        return { team: teamInfo, members };
    }
}
```

## Performance and Scalability

### Caching Strategies
```javascript
/**
 * Intelligent caching for agent responses and data
 * @class AgentCacheManager
 */
class AgentCacheManager {
    /**
     * Implements multi-level caching for optimal performance
     * @param {string} key - Cache key
     * @param {Function} dataProvider - Function to fetch data if not cached
     * @returns {Promise<Object>} Cached or freshly fetched data
     */
    async getOrSet(key, dataProvider, ttl = 3600) {
        let data = await this.memoryCache.get(key);
        
        if (!data) {
            data = await this.distributedCache.get(key);
            if (data) {
                await this.memoryCache.set(key, data, ttl / 4);
            }
        }
        
        if (!data) {
            data = await dataProvider();
            await this.setMultiLevel(key, data, ttl);
        }
        
        return data;
    }
}
```

### Asynchronous Processing
```javascript
/**
 * Background task processing for complex operations
 * @param {Object} task - Task to be processed
 * @param {TurnContext} context - Bot context for updates
 */
async processLongRunningTask(task, context) {
    // Send immediate acknowledgment
    await context.sendActivity('Processing your request...');
    
    // Process in background
    setImmediate(async () => {
        try {
            const result = await this.executeLongTask(task);
            await this.sendDelayedResponse(context, result);
        } catch (error) {
            await this.sendErrorResponse(context, error);
        }
    });
}
```

## Monitoring and Analytics

### Agent Performance Metrics
```javascript
/**
 * Tracks agent performance and user satisfaction
 * @class AgentAnalytics
 */
class AgentAnalytics {
    /**
     * Records interaction metrics for analysis
     * @param {Object} interaction - Interaction data
     */
    async recordInteraction(interaction) {
        const metrics = {
            responseTime: interaction.responseTime,
            userSatisfaction: interaction.satisfaction,
            intentAccuracy: interaction.intentAccuracy,
            timestamp: new Date().toISOString()
        };
        
        await this.metricsStore.record(metrics);
    }
}
```

### Health Monitoring
```javascript
/**
 * Health check endpoint for agent monitoring
 * @param {Request} req - HTTP request
 * @param {Response} res - HTTP response
 */
function healthCheck(req, res) {
    const health = {
        status: 'healthy',
        timestamp: new Date().toISOString(),
        services: {
            bot: this.botService.isHealthy(),
            cache: this.cacheService.isHealthy(),
            database: this.dbService.isHealthy()
        }
    };
    
    res.json(health);
}
```

## Testing Strategies

### Agent Behavior Testing
```javascript
/**
 * Tests agent responses for various scenarios
 * @class AgentBehaviorTests
 */
class AgentBehaviorTests {
    /**
     * Tests agent response quality and consistency
     */
    async testResponseQuality() {
        const testScenarios = await this.loadTestScenarios();
        
        for (const scenario of testScenarios) {
            const response = await this.agent.processMessage(scenario.input);
            const quality = await this.evaluateResponse(response, scenario.expected);
            
            assert(quality.score > 0.8, 'Response quality below threshold');
        }
    }
}
```

This comprehensive framework provides patterns for building intelligent, scalable Microsoft Teams agents that integrate seamlessly with the Teams platform while maintaining high performance and user satisfaction.
