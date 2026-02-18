# Auth Bot

A Microsoft Teams bot with SSO authentication and Graph-based proactive app installation.

## Features

- **SSO Authentication** - Single Sign-On with Microsoft Entra ID
- **Proactive Messaging** - Install the bot and send notifications to team members via Microsoft Graph

## Bot Commands

| Command | Description |
|---------|-------------|
| `login` | Sign in using SSO |
| `logout` | Sign out from the bot |
| `install` | Install the app for all team/group chat members |
| `send` | Send proactive messages to all team/group chat members |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022 v17.14+](https://visualstudio.microsoft.com/downloads/) with Microsoft 365 Agents Toolkit

## Run the sample

### Using Microsoft 365 Agents Toolkit (Recommended)

1. Open the solution in Visual Studio
2. Right-click the `M365Agent` project and select **Microsoft 365 Agents Toolkit > Prepare Teams App Dependencies**
3. Press `F5` to start debugging

### Using Command Line

1. Navigate to this directory:

```sh
cd BotAuthQuickstart
```

2. Restore dependencies and run:

```sh
dotnet run
```

The bot will start listening on `http://localhost:5130`.

## Further Reading

See the [root README](../README.md) for detailed setup and configuration instructions.

