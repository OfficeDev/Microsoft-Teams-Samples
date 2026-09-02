# Agent Targeted Messages - .NET (C#)

This sample demonstrates how to send **targeted messages** in Microsoft Teams using the Teams SDK for .NET. Targeted messages are only visible to a specific user in a channel or group chat.

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
   cd dotnet/agent-targeted-messages
   ```

2. Restore dependencies and run:
   ```bash
   dotnet run
   ```

The agent will start listening on `http://localhost:3978`.

Refer to the main [README.md](../../README.md) to interact with your agent in the agentsplayground or in Teams.