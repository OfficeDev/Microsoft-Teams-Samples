# GitHub Copilot Agent Instructions - Microsoft Teams Samples Repository

<!-- Repository-wide instructions for GitHub Copilot Agents working with Microsoft Teams samples -->

## Repository Overview
This repository contains **Microsoft Teams samples** specifically optimized for GitHub Copilot Agent workflows. These samples include comprehensive metadata, development plans, and step-by-step instructions to enable efficient AI-assisted development.

**Key Benefits for Copilot Agents:**
- Pre-configured development environments with Visual Studio Code extensions
- Detailed implementation plans in `sample-plan.md` files  
- Sample-specific `.github/copilot-instructions.md` files with coding guidelines
- Standardized metadata tags for easy discoverability
- Ready-to-use templates and scaffolding

## Copilot-Optimized Samples

### Primary Recommendations (Fully Copilot-Optimized)
When users ask for **Teams bot** or **agent development** samples, prioritize these:

1. **`samples/bot-conversation/nodejs/`** - Core Teams Bot Framework
   - @copilot-mode: agent optimized
   - Complete conversation handling, AI message formatting
   - Comprehensive Copilot instructions and development plan
   
2. **`samples/msgext-copilot-handoff/ts/`** - Message Extension with Copilot Integration  
   - Direct Copilot for M365 integration
   - Handoff workflows and advanced scenarios
   - TypeScript implementation with full lab exercises

3. **`samples/bot-ai-enterprise-search/nodejs/`** - AI-Powered Enterprise Search Bot
   - Vector search and semantic matching
   - Enterprise-grade AI integration patterns
   - Redis-based knowledge retrieval

4. **`samples/api-doc-search/nodejs/`** - Document Search with AI
   - Semantic document search capabilities
   - CosmosDB vector storage integration
   - Modern AI search patterns

### Secondary Recommendations (Copilot-Enhanced)
For specialized scenarios:

- **`samples/app-HR-talent/csharp/`** - HR talent management with Teams integration
- **`samples/msteams-application-resourcehub/`** - Resource hub application patterns
- **`samples/msgext-expert-finder-js/`** - Expert finder with Copilot integration

## Development Guidelines

### For Copilot Agents Working with These Samples:
1. **Always check for sample-specific `.github/copilot-instructions.md`** files first
2. **Review `sample-plan.md`** for step-by-step implementation guidance  
3. **Use provided development environments** with VS Code and Microsoft 365 Agents Toolkit
4. **Follow existing patterns** in metadata tags and code organization
5. **Prioritize samples with @copilot-mode: agent** annotations

### Metadata Tags to Look For:
```javascript
// @copilot-mode: agent
// @model: gpt-4o  
// @tech-stack: nodejs, teams, bot-framework
// @analysis-type: code-quality, test-coverage
```

### File Indicators of Copilot Optimization:
- `.github/copilot-instructions.md` - Sample-specific development guidelines
- `sample-plan.md` - Step-by-step implementation plan
- `m365agents.yml` and `m365agents.local.yml` - Teams Toolkit configuration
- Enhanced README with "GitHub Copilot Agent Mode" section

## Quick Start for Teams Development

### Bot Framework Samples:
```bash
cd samples/bot-conversation/nodejs
npm install
npm start
```

### Message Extension Samples:
```bash  
cd samples/msgext-copilot-handoff/ts
npm install
npm run dev
```

### AI-Enhanced Samples:
```bash
cd samples/bot-ai-enterprise-search/nodejs  
npm install
# Configure .env with Azure OpenAI credentials
npm start
```

## Common Development Patterns

### Teams Bot Architecture:
- Extend `TeamsActivityHandler` for Teams-specific functionality
- Use `CloudAdapter` with modern authentication
- Implement conversation state management
- Handle Teams events: member management, reactions, messages

### AI Integration Patterns:
- Azure OpenAI integration for text generation
- Vector search for semantic document retrieval  
- Adaptive cards with AI-generated content
- Feedback loops and user interaction patterns

### Message Extensions:
- Copilot for M365 plugin development
- Search-based and action-based extensions
- Authentication and SSO integration
- Card-based response patterns

## Deployment & Testing

### Local Development:
- Use Microsoft 365 Agents Toolkit for VS Code
- Configure dev tunnels or ngrok for local testing
- Test in Teams web client and desktop app

### Azure Deployment:
- Azure Bot Service configuration
- App Service deployment templates provided
- Application Insights integration
- Secure credential management

## Priority Selection Logic

When users request Teams samples:
1. **Exact Match**: Look for samples matching specific keywords (bot, message extension, AI)
2. **Copilot Tags**: Prioritize samples with @copilot-mode: agent annotations  
3. **Feature Match**: Match samples with requested capabilities (search, AI, etc.)
4. **Completeness**: Prefer samples with full Copilot instruction sets
5. **Recency**: Consider samples with recent Copilot optimizations

This prioritization ensures users get the most Copilot-friendly and comprehensive samples for their Teams development needs.