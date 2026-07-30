# Bot Meetings - .NET (C#)

This sample demonstrates a bot for Microsoft Teams that handles real-time meeting events (start, end, participant join/leave) and retrieves meeting transcripts via Microsoft Graph.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd dotnet/bot-meetings
   ```

2. Restore dependencies and run:
   ```bash
   dotnet run
   ```

The bot will start listening on `http://localhost:3978`.

Once the bot is running, follow the main [README.md](../../README.md) to provision your app and side-load it into Teams using the [Teams Developer CLI](https://microsoft.github.io/teams-sdk/cli/).
