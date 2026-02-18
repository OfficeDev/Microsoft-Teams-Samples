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

- [Python 3.12+](https://www.python.org/downloads/)
- [Visual Studio Code](https://code.visualstudio.com/) with [Microsoft 365 Agents Toolkit](https://aka.ms/teams-toolkit) extension

## Run the sample

### Using Microsoft 365 Agents Toolkit (Recommended)

1. Open the folder in VS Code
2. Select the Microsoft 365 Agents Toolkit icon on the left toolbar
3. Sign in with your Microsoft 365 account
4. Press `F5` to start debugging

### Using Command Line

1. Navigate to this directory:

```sh
cd python/bot-auth-quickstart
```

2. Install dependencies and run:

```sh
uv sync
uv run main.py
```

The bot will start listening on `http://localhost:3978`.

## Further Reading

See the [root README](../../README.md) for detailed setup and configuration instructions.
