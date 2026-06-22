# Bot Attachments - .NET (C#)

This sample demonstrates how to send and receive file attachments in Microsoft Teams using a bot built with the Teams SDK. When a user sends a file, the bot downloads it, requests consent via a File Consent Card, and uploads the file to the user's OneDrive upon acceptance.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd dotnet/bot-attachments
   ```

2. Copy the example launch settings file:
   ```bash
   cp Properties/launchSettings.EXAMPLE.json Properties/launchSettings.json
   ```
   
   Update `Properties/launchSettings.json` with your Teams app credentials (TenantId, ClientId, ClientSecret).

3. Restore dependencies and run:
   ```bash
   dotnet run
   ```

The bot will start listening on `http://localhost:3978`.

## Features

- **File receive**: Accepts files sent by the user in a Teams chat.
- **File Consent Card**: Requests user permission before uploading to OneDrive.
- **OneDrive upload**: On acceptance, uploads the file and sends a File Info Card with a link.
- **Decline handling**: Notifies the user gracefully when consent is declined.

## Next Steps

Refer to the [main README](../../README.md) for instructions on how to:
- Deploy and test your bot in Microsoft Teams
- Configure Teams app manifest and credentials
