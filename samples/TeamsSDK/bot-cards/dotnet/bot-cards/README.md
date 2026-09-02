# Bot Cards - .NET (C#)

This sample demonstrates how to interact with adaptive cards in Microsoft Teams using a bot built with Teams SDK.

## Features

- **Card Actions** — Adaptive card with `Action.OpenUrl`, `Action.Submit`, and nested `Action.ShowCard` behaviours.
- **Toggle Visibility** — Adaptive card with `Action.ToggleVisibility` to show or hide content.

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
   cd dotnet/bot-cards
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

## Bot commands

| Command | Description |
|---|---|
| `card actions` | Sends an adaptive card showcasing various card actions |
| `toggle visibility` | Sends a card demonstrating `Action.ToggleVisibility` |

## Next Steps

Refer to the [main README](../../README.md) for instructions on how to:
- Deploy and test your bot in Microsoft Teams
- Configure Teams app manifest and credentials
