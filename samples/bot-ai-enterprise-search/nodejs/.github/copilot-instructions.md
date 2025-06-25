# GitHub Copilot Agent Instructions - Bot AI Enterprise Search Sample (Node.js/TypeScript)

## Project Overview
This sample demonstrates an **AI-powered enterprise search bot** for Microsoft Teams using Bot Framework, Azure OpenAI, and Redis vector database. It enables users to perform semantic search across enterprise documents through natural language conversations within Teams.

**Key Features:**
- Teams bot with conversational AI integration
- Vector embeddings and semantic search with Azure OpenAI
- Redis vector database for fast document retrieval
- Enterprise-grade search with contextual responses
- Proactive messaging and user interaction patterns

## Architecture & File Structure
```
/
├── index.ts                  # Main bot server and application logic
├── package.json              # Dependencies and scripts
├── .env                      # Environment configuration
├── tsconfig.json             # TypeScript configuration
└── README.md                 # Setup and usage documentation
```

## Coding Guidelines

### TypeScript Standards
- Use **TypeScript** with strict type checking enabled
- Prefer `async/await` for asynchronous operations  
- Use proper type annotations for all functions and variables
- Implement comprehensive error handling with typed exceptions

### Bot Framework Patterns
- Import from `botbuilder` package for core bot functionality
- Use `CloudAdapter` with `ConfigurationBotFrameworkAuthentication`
- Implement proper message handling with activity types
- Handle bot lifecycle events appropriately

### AI Integration Best Practices
- Use Azure OpenAI SDK for embeddings and completions
- Implement vector search with Redis for fast retrieval
- Generate contextual responses with proper prompt engineering
- Handle AI service errors gracefully with fallback responses

## Environment Configuration
```env
# Bot Framework Configuration
MicrosoftAppId=your_microsoft_app_id
MicrosoftAppPassword=your_microsoft_app_password
MicrosoftAppTenantId=your_tenant_id
MicrosoftAppType=MultiTenant

# Azure OpenAI Configuration
AZURE_OPENAI_API_KEY=your_azure_openai_key
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4
AZURE_OPENAI_EMBEDDING_DEPLOYMENT=text-embedding-ada-002

# Redis Configuration
REDIS_CONNECTION_STRING=your_redis_connection_string
REDIS_INDEX_NAME=document_index

# Server Configuration
PORT=3978
```

## Core Dependencies & Imports
```typescript
// Bot Framework imports
import {
  CloudAdapter,
  ConfigurationBotFrameworkAuthentication,
  TurnContext,
  ActivityTypes,
  MessageFactory
} from "botbuilder";

// Azure OpenAI imports
import { OpenAIClient, AzureKeyCredential } from "@azure/openai";

// Redis imports
import * as redis from "redis";

// Server setup
import * as restify from "restify";
```

## AI-Powered Search Implementation

### Embedding Generation Pattern
```typescript
async function getEmbeddingAsync(text: string): Promise<number[]> {
  const client = new OpenAIClient(
    process.env.AZURE_OPENAI_ENDPOINT!,
    new AzureKeyCredential(process.env.AZURE_OPENAI_API_KEY!)
  );
  
  const embeddings = await client.getEmbeddings(
    process.env.AZURE_OPENAI_EMBEDDING_DEPLOYMENT!,
    [text]
  );
  
  return embeddings.data[0].embedding;
}
```

### Vector Search with Redis
```typescript
async function searchInDBAsync(
  context: TurnContext, 
  userPromptEmbedding: number[], 
  userPrompt: string
): Promise<void> {
  const baseQuery = "*=>[KNN 5 @context_vector $context_vector AS vector_score]";
  const returnFields = ["context_vector", "context", "docUrl", "vector_score"];
  
  const results = await redisClient.ft.search(INDEX_NAME, baseQuery, {
    PARAMS: {
      context_vector: float32Buffer(userPromptEmbedding)
    },
    SORTBY: 'vector_score',
    DIALECT: 2,
    RETURN: returnFields,
  });
  
  await parseResultAndCallCompletionAsync(context, results, userPrompt);
}
```

### AI Response Generation
```typescript
async function parseResultAndCallCompletionAsync(
  context: TurnContext,
  results: any,
  userPrompt: string
): Promise<void> {
  const contextData = results.documents
    .map((doc: any) => doc.value.context)
    .join('\n\n');
  
  const promptText = `Based on the following context, answer the user's question:
    
Context: ${contextData}

Question: ${userPrompt}

Answer:`;

  const completion = await openAIClient.getChatCompletions(
    process.env.AZURE_OPENAI_DEPLOYMENT_NAME!,
    [{ role: "user", content: promptText }]
  );
  
  const answer = completion.choices[0].message?.content || "I couldn't generate a response.";
  await context.sendActivity(MessageFactory.text(answer));
}
```

## Bot Implementation Pattern

### Main Bot Class Structure
```typescript
class EnterpriseSearchBot {
  constructor() {
    // Initialize bot handlers
    this.onMessage(async (context, next) => {
      await this.handleMessage(context);
      await next();
    });
  }
  
  private async handleMessage(context: TurnContext): Promise<void> {
    const userQuery = context.activity.text;
    const embedding = await getEmbeddingAsync(userQuery);
    await searchInDBAsync(context, embedding, userQuery);
  }
}
```

### Server Setup Pattern
```typescript
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Error handling
adapter.onTurnError = async (context, error) => {
  console.error(`\n [onTurnError] unhandled error: ${error}`);
  await context.sendActivity("Sorry, something went wrong with your search.");
};

// Create bot
const bot = new EnterpriseSearchBot();

// Setup server
const server = restify.createServer();
server.use(restify.plugins.bodyParser());

server.post('/api/messages', async (req, res) => {
  await adapter.process(req, res, async (context) => {
    await bot.run(context);
  });
});
```

## Redis Vector Database Setup

### Index Creation
```typescript
async function createRedisIndex(): Promise<void> {
  try {
    await redisClient.ft.create(INDEX_NAME, {
      '$.context': {
        type: SchemaFieldTypes.TEXT,
        AS: 'context'
      },
      '$.context_vector': {
        type: SchemaFieldTypes.VECTOR,
        ALGORITHM: VectorAlgorithms.HNSW,
        TYPE: 'FLOAT32',
        DIM: 1536,
        DISTANCE_METRIC: 'COSINE',
        AS: 'context_vector'
      }
    }, {
      ON: 'JSON',
      PREFIX: 'doc:'
    });
  } catch (error) {
    console.log('Index may already exist:', error);
  }
}
```

## Advanced Features

### Conversational Context Management
- Maintain conversation history for multi-turn interactions
- Track user search patterns and preferences
- Implement conversation state management
- Handle context-aware follow-up questions

### Enterprise Integration
- Integrate with Azure AD for authentication
- Connect to SharePoint, OneDrive, or other enterprise systems
- Implement document access control and permissions
- Add audit logging for search queries

### Performance Optimization
- Cache frequently accessed documents and embeddings
- Implement connection pooling for Redis and OpenAI
- Optimize vector search parameters and indexing
- Monitor response times and adjust accordingly

## Testing & Debugging

### Development Setup
- Use `npm run dev` for development with hot reload
- Test bot in Bot Framework Emulator first
- Deploy to ngrok or dev tunnels for Teams testing
- Monitor Redis and OpenAI usage with logging

### Common Development Tasks
- **Add new document sources**: Extend data ingestion pipeline
- **Improve search relevance**: Tune vector search parameters and prompts
- **Enhance bot responses**: Modify AI prompt templates and response formatting
- **Add authentication**: Integrate Azure AD for enterprise security

## Performance Considerations
- Vector searches can be CPU intensive - monitor Redis performance
- OpenAI API calls have rate limits - implement retry logic
- Embedding generation can be slow for large documents
- Consider caching strategies for frequently accessed content

## Security Best Practices
- Store all secrets in environment variables or Azure Key Vault
- Implement proper input validation and sanitization
- Use managed identities for Azure service authentication
- Monitor and log all search queries for compliance

## Error Handling Patterns
```typescript
try {
  const results = await searchInDBAsync(context, embedding, query);
  await sendResponseAsync(context, results);
} catch (error) {
  console.error('Search error:', error);
  await context.sendActivity(MessageFactory.text(
    "I'm having trouble searching right now. Please try again later."
  ));
}
```

Remember to follow existing TypeScript patterns and implement proper error handling for all AI and database operations.