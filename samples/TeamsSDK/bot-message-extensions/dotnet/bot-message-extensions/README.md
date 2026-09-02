# Bot Message Extensions - .NET (C#)

This sample demonstrates a search-based messaging extension in Microsoft Teams that allows users to search for Wikipedia articles.

It targets [Teams SDK for .NET 2.1](https://microsoft.github.io/teams-sdk/csharp/getting-started/quickstart) (`Microsoft.Teams.Apps`).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Configuration

Bot credentials use the MSAL-native `AzureAd` configuration section in [appsettings.json](appsettings.json):

```json
{
  "AzureAd": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientCredentials": [
      { "SourceType": "ClientSecret", "ClientSecret": "your-client-secret" }
    ]
  }
}
```

For local testing with the Microsoft 365 Agents Playground (which sends unauthenticated requests), set `AzureAd__DangerouslyAllowUnauthenticatedRequests` to `true` in your launch profile. Never enable this in production.

## Run the sample

1. Navigate to this directory:
   ```bash
   cd dotnet/bot-message-extensions
   ```

2. Restore dependencies and run:
   ```bash
   dotnet run
   ```

The bot will start listening on `http://localhost:3978`.

Once the bot is running, follow the main [README.md](../../README.md) to provision your app and side-load it into Teams using the [Teams Developer CLI](https://microsoft.github.io/teams-sdk/cli/).
