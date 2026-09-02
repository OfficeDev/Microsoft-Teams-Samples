# Bot Auth Quickstart - .NET (C#)

A Microsoft Teams bot with SSO authentication and Microsoft Graph integration.

## Features

- **SSO Authentication** - Single Sign-On with Microsoft Entra ID
- **Graph Integration** - Fetch user profile via Microsoft Graph

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

```sh
cd dotnet/bot-auth-quickstart
```

2. Run the bot:

```bash
dotnet run
```

The bot will start listening on `http://localhost:3978`.

## Further Reading

Refer to the main [Root README](../../../README.md) for detailed setup and configuration instructions.

