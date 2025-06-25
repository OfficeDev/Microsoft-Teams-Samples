# GitHub Copilot Agent Instructions - API Document Search Sample (Node.js)

## Project Overview
This sample demonstrates **semantic document search** using Azure OpenAI embeddings and Azure Cosmos DB vector storage. It provides a web interface for users to search through documents using natural language queries, leveraging vector similarity matching for accurate results.

**Key Features:**
- Vector embeddings generation with Azure OpenAI
- Semantic document search with similarity scoring
- Azure Cosmos DB vector storage and retrieval
- Application Insights telemetry integration
- Express.js web application with EJS templating

## Architecture & File Structure
```
/
├── app.js                    # Main Express application setup
├── routes/
│   ├── index.js             # Home page route handler
│   └── search.js            # Vector search implementation
├── views/                   # EJS templates for UI
├── public/                  # Static assets (CSS, JS, images)
├── package.json             # Dependencies and scripts
└── .env                     # Environment configuration
```

## Coding Guidelines

### TypeScript/JavaScript Standards
- Use **modern JavaScript** (ES6+ features)
- Prefer `async/await` for asynchronous operations
- Use destructuring for cleaner imports: `const { OpenAIClient, AzureKeyCredential } = require("@azure/openai");`
- Implement comprehensive error handling with try-catch blocks

### Azure Integration Patterns
- Use `@azure/openai` SDK for embeddings generation
- Use `@azure/cosmos` SDK for vector storage and retrieval
- Configure Application Insights for telemetry and monitoring
- Implement proper environment variable management

### Search Implementation Best Practices
- Generate embeddings for search queries using Azure OpenAI
- Use vector similarity search with configurable similarity thresholds
- Implement top-K retrieval with sorting by similarity scores
- Add comprehensive logging for search operations and results

## Environment Configuration
```env
# Azure OpenAI Configuration
AZURE_OPENAI_API_KEY=your_azure_openai_key
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_DEPLOYMENT_NAME=text-embedding-ada-002

# Azure Cosmos DB Configuration
COSMOS_DB_ENDPOINT=https://your-cosmosdb.documents.azure.com:443/
COSMOS_DB_KEY=your_cosmos_db_key
COSMOS_DB_DATABASE_NAME=your_database
COSMOS_DB_CONTAINER_NAME=your_container

# Search Configuration
SIMILARITY_SCORE=0.7

# Application Insights
APPINSIGHTS_INSTRUMENTATIONKEY=your_instrumentation_key
APPINSIGHTS_CONNECTIONSTRING=your_connection_string
```

## Core Dependencies & Imports
```javascript
// Essential Azure SDK imports
const { OpenAIClient, AzureKeyCredential } = require("@azure/openai");
const { CosmosClient } = require("@azure/cosmos");

// Express and utilities
const express = require('express');
const path = require('path');
require('dotenv').config();

// Application Insights
const appInsights = require('applicationinsights');
```

## Vector Search Implementation Pattern

### Embedding Generation
```javascript
async function getEmbeddingAsync(query) {
    const client = new OpenAIClient(
        process.env.AZURE_OPENAI_ENDPOINT,
        new AzureKeyCredential(process.env.AZURE_OPENAI_API_KEY)
    );
    
    const embeddings = await client.getEmbeddings(
        process.env.AZURE_OPENAI_DEPLOYMENT_NAME,
        [query]
    );
    
    return embeddings.data[0].embedding;
}
```

### Vector Search Query
```javascript
async function semanticSearchDocumentsAsync(query) {
    const embedding = await getEmbeddingAsync(query);
    const similarityScore = Number.parseFloat(process.env.SIMILARITY_SCORE);
    
    const queryText = `
        SELECT TOP 5 c.contents, c.fileName, c.url,
        VectorDistance(c.vectors, @vectors, false) as similarityScore
        FROM c
        WHERE VectorDistance(c.vectors, @vectors, false) > @similarityScore`;
    
    const querySpec = {
        query: queryText,
        parameters: [
            { name: "@vectors", value: embedding },
            { name: "@similarityScore", value: similarityScore }
        ]
    };
    
    const { resources } = await container.items.query(querySpec).fetchAll();
    return resources.sort((a, b) => b.similarityScore - a.similarityScore);
}
```

## Application Structure

### Express App Setup Pattern
```javascript
const app = express();

// View engine setup
app.set('views', path.join(__dirname, 'views'));
app.set('view engine', 'ejs');

// Middleware
app.use(express.json());
app.use(express.urlencoded({ extended: false }));
app.use(express.static(path.join(__dirname, 'public')));

// Routes
app.use('/', indexRouter);
app.use('/search', searchRouter);
```

### Route Handler Pattern
```javascript
router.post('/', async (req, res) => {
    try {
        const query = req.body.query;
        const results = await semanticSearchDocumentsAsync(query);
        
        res.render('results', { 
            query: query, 
            results: results,
            hasResults: results.length > 0
        });
    } catch (error) {
        console.error('Search error:', error);
        res.status(500).render('error', { error: error });
    }
});
```

## Advanced Features

### Telemetry Integration
- Track search queries and results with Application Insights
- Monitor embedding generation performance
- Log vector similarity scores and search effectiveness
- Implement custom events for search analytics

### Performance Optimization
- Cache frequently used embeddings
- Implement query result caching
- Optimize Cosmos DB vector queries
- Monitor and tune similarity thresholds

### Error Handling
- Graceful handling of Azure service failures
- Retry logic for transient errors
- User-friendly error messages
- Comprehensive logging for debugging

## Testing & Debugging

### Development Setup
- Use `npm start` to run the application locally
- Test search functionality with various query types
- Monitor Application Insights for telemetry data
- Validate vector similarity scores and result relevance

### Common Development Tasks
- **Add new document types**: Extend vector storage schema
- **Improve search accuracy**: Tune similarity thresholds and embedding models
- **Enhance UI**: Modify EJS templates and static assets
- **Add authentication**: Integrate Azure AD or other auth providers

## Performance Considerations
- Embedding generation can be slow - consider caching strategies
- Vector searches scale with document count - implement pagination
- Monitor Cosmos DB RU consumption
- Optimize query parameters for cost-effectiveness

## Security Best Practices
- Store all secrets in environment variables
- Use Azure Key Vault for production secrets
- Implement proper input validation
- Configure CORS appropriately for production

Remember to follow existing patterns when extending search functionality or adding new document types.