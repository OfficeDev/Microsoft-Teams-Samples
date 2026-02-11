# Bot Auth Quickstart - Python

This sample demonstrates implementing SSO (Single Sign-On) in Microsoft Teams using the Microsoft Teams SDK V2 and Azure AD. It also includes proactive app installation and messaging capabilities.

This app template is built on top of [Microsoft Teams SDK](https://aka.ms/teams-ai-library-v2).

## Features

- **SSO Authentication**: Single Sign-On using Azure AD
- **User Profile Display**: Shows user's display name, email, job title, and profile photo
- **Token Display**: Option to view the access token
- **Proactive App Installation**: Install the app for all team members via Graph API
- **Proactive Messaging**: Send messages to all team members

## Prerequisites

- [Python 3.12+](https://www.python.org/downloads/)
- [uv](https://docs.astral.sh/uv/) (recommended) or pip
- [Microsoft 365 Agents Toolkit Visual Studio Code Extension](https://aka.ms/teams-toolkit) latest version or [Microsoft 365 Agents Toolkit CLI](https://aka.ms/teams-toolkit-cli).
- A [Microsoft 365 account for development](https://docs.microsoft.com/microsoftteams/platform/toolkit/accounts).

## Run the sample

### Using uv (recommended)

1. Install dependencies:
   ```bash
   uv sync
   ```

2. Run the bot:
   ```bash
   uv run main.py
   ```

### Using pip

1. Install dependencies:
   ```bash
   pip install -e .
   ```

2. Run the bot:
   ```bash
   python main.py
   ```

The bot will start listening on `http://localhost:3978`.

## Run with Teams Toolkit

1. Select the Microsoft 365 Agents Toolkit icon on the left in the VS Code toolbar.
2. In the Account section, sign in with your [Microsoft 365 account](https://docs.microsoft.com/microsoftteams/platform/toolkit/accounts) if you haven't already.
3. Press F5 to start debugging which launches your app in Teams using a web browser. Select `Debug in Teams (Edge)` or `Debug in Teams (Chrome)`.
4. When Teams launches in the browser, select the Add button in the dialog to install your app to Teams.

## Interacting with the bot

| Command | Description |
|---------|-------------|
| `login` | Sign in with SSO authentication |
| `logout` | Sign out from the bot |
| `install` | Install the app for all team members in their personal scope |
| `send` | Send a proactive message to all team members |
| `yes` | View your access token (after login) |
| `no` | Cancel viewing the token |

### Authentication Flow
1. Type `login` to sign in with SSO authentication.
2. After signing in, you'll see your profile photo and display name.
3. Type `Yes` to view your access token, or `No` to cancel.
4. Type `logout` to sign out.

### Proactive Installation (in Teams/Group Chat)
1. Type `install` to install the app for all team members.
2. The bot will install the app in each member's personal scope.
3. A summary of installations will be displayed.

### Proactive Messaging (in Teams/Group Chat)
1. Type `send` to send proactive messages.
2. The bot will send a message to each team member's personal chat.

# Additional information and references

- [Microsoft 365 Agents Toolkit Documentations](https://docs.microsoft.com/microsoftteams/platform/toolkit/teams-toolkit-fundamentals)
- [Microsoft 365 Agents Toolkit CLI](https://aka.ms/teamsfx-toolkit-cli)
- [Microsoft Teams SDK Documentation](https://learn.microsoft.com/microsoftteams/platform/)
