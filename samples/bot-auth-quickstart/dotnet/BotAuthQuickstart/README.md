# Auth Bot

A Microsoft Teams bot with SSO authentication and Microsoft Graph API integration.

## Features

- **SSO Authentication** - Single Sign-On with Microsoft Entra ID
- **Graph API Integration** - Retrieve user profile information


## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- One of the following:
  - [Visual Studio 2026](https://visualstudio.microsoft.com/downloads/) with Microsoft 365 Agents Toolkit
  - [VS Code](https://code.visualstudio.com/) with Teams Toolkit extension
  - .NET CLI (no IDE required)

## Run the sample

> **Note:** This sample uses SSO authentication and therefore **does not work** in Microsoft 365 Agents Playground. You must test it in Microsoft Teams.


1. **Navigate to this directory and run:**

```sh
cd BotAuthQuickstart
dotnet run
```

The bot will start listening on `http://localhost:5130`.

## Further Reading

See the [root README](../../README.md) for detailed setup and configuration instructions.

