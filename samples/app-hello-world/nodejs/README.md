---
page_type: sample
products:
- office-365
- microsoft-teams
languages:
- javascript
- nodejs
technologies:
- botframework
- teams-platform
- messaging-extensions
- teams-tabs
title: Microsoft Teams Hello World Bot - Copilot Agent Optimized
description: A comprehensive Microsoft Teams hello world bot sample optimized for GitHub Copilot Agent development, featuring advanced bot functionality, tabs, and messaging extensions with modern JavaScript patterns.
extensions:
  contentType: samples
  technologies:
    - Microsoft Teams Platform
    - Bot Framework v4
    - Node.js
    - Restify
    - GitHub Copilot Agent
  createdDate: 10/19/2022 10:02:21 PM
  updatedDate: 06/26/2025 12:00:00 PM
urlFragment: officedev-microsoft-teams-samples-app-hello-world-nodejs-copilot-optimized
copilot:
  optimized: true
  agent_patterns:
    - teams-bot-development
    - conversational-ai
    - messaging-extensions
    - teams-tabs
  useCases:
    - echo-bot-implementation
    - teams-integration-patterns
    - bot-state-management
    - error-handling-best-practices
---

# Microsoft Teams Hello World Bot - GitHub Copilot Agent Optimized 🤖

A **comprehensive Microsoft Teams bot sample** that showcases fundamental Teams platform features including **tabs**, **bots**, and **messaging extensions**. This sample has been **optimized for GitHub Copilot Agent development** with enhanced code structure, comprehensive documentation, and modern development patterns.

## 🤖 GitHub Copilot Agent Mode

This sample is optimized for GitHub Copilot Agent development with:

- **Teams-first development patterns** prioritized over generic Bot Framework
- **Enhanced discoverability** through project structure and documentation
- **Copilot-specific guidance** in `.github/copilot-instructions.md`
- **Development patterns** and examples in `.copilot/` directory

### Quick Start with Copilot
1. Use Copilot prompts for Teams bot development
2. Reference `.copilot/prompts/hello-world.md` for common patterns
3. Follow Teams-specific coding standards in development guidelines

## 🚀 Features & Capabilities

### Core Microsoft Teams Integration
- ✅ **Echo Bot Functionality** - Intelligent message processing with context awareness
- ✅ **Teams Tabs** - Static and configurable tab experiences  
- ✅ **Messaging Extensions** - Search-based extensions with dynamic card generation
- ✅ **State Management** - Conversation state with proper error handling
- ✅ **Teams Activity Handler** - Full Teams platform integration

### GitHub Copilot Agent Optimizations
- 🎯 **Enhanced Code Discoverability** - Semantic naming and modular architecture
- 📚 **Comprehensive JSDoc** - Detailed inline documentation for better AI understanding
- 🔧 **Modular File Structure** - Clean separation of concerns for easy code navigation
- 🤖 **Agent-Friendly Patterns** - Code patterns optimized for AI assistance and generation
- 📋 **Copilot Prompts** - Pre-configured prompts and instructions in `.copilot/` directory

## 🏗️ Architecture & Code Structure

```
src/
├── app.js                         # 🚀 Main application entry point
├── bot.js                         # 🤖 Bot functionality and handlers
├── tabs.js                        # 📑 Teams tab management and routing
├── message-extension.js           # 💬 Messaging extension components
├── static/                        # 🎨 Static assets and content
└── views/                         # 📄 HTML templates for tabs

.copilot/                          # 🧠 Copilot Agent optimization
├── prompts/                       # 📝 Copilot-specific prompts
│   └── hello-world.md            # Development guidance and patterns
└── instructions/                  # 📖 Development guidelines
    ├── development-guidelines.md  # Coding standards and best practices
    └── agent-patterns.md         # Advanced agent development patterns

.github/                           # 🔧 GitHub integration
└── copilot-instructions.md       # Teams-first development guidance
```

## 💻 Development Environment & Prerequisites

### Prerequisites
- ✅ **Microsoft Teams** account (not guest account)
- ✅ **Node.js 16.14.2+** for development
- ✅ **Dev tunnel** or **ngrok** for local testing
- ✅ **M365 developer account** or Teams account with app upload permissions
- ✅ **Microsoft 365 Agents Toolkit for VS Code** (recommended)

### GitHub Copilot Integration
This sample is **optimized for GitHub Copilot Agent Mode** with:
- � **`.github/copilot-instructions.md`** - Teams-first development guidance
- 🤖 **`.copilot/` directory** - Agent-specific prompts and patterns
- 📚 **Comprehensive JSDoc** - Rich context for AI assistance
- 🎯 **Teams-specific patterns** - Prioritized over generic Bot Framework

## �🚀 Quick Start Options

### Option 1: Microsoft 365 Agents Toolkit (Recommended)

The **fastest way** to run this Teams sample:

1. **Install VS Code** and [Microsoft 365 Agents Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
2. **Open this folder** in VS Code
3. **Sign in** with your Microsoft 365 account
4. **Press F5** or select **Debug > Start Debugging**
5. **Click Add** in the browser to install to Teams

### Option 2: Manual Setup with Teams Focus

> 🎯 **Teams-First Approach**: This setup prioritizes Teams-specific patterns over generic Bot Framework

### 1. Clone and Install
```bash
git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
cd samples/app-hello-world/nodejs
npm install
```

### 2. Configure Environment
1. Register a new bot at [Microsoft Bot Framework](https://dev.botframework.com/bots/new)
2. Create `.env` file with your bot credentials:
```
MICROSOFT_APP_ID=your-app-id
MICROSOFT_APP_PASSWORD=your-app-password
```

### 3. Start Local Development
```bash
npm start
```

### 4. Setup Teams Manifest
1. Update `appManifest/manifest.json` with your App ID
2. Replace `{{domain-name}}` with your tunnel domain (e.g., `1234.ngrok-free.app`)
3. Zip the `appManifest` folder contents
4. Upload to Teams via Apps > Manage your apps > Upload an app

## 📚 Learn More

- [Microsoft Teams Platform Documentation](https://docs.microsoft.com/microsoftteams/platform/)
- [Bot Framework v4 SDK](https://docs.microsoft.com/azure/bot-service/)
- [Teams Samples Repository](https://github.com/OfficeDev/Microsoft-Teams-Samples)

## 🤝 Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA).

## 📄 License

This sample is licensed under the MIT License.

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/app-hello-world-nodejs" />
