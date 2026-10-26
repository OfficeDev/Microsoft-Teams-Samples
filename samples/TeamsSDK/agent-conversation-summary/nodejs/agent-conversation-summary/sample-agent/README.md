# sample-agent

A minimal Microsoft Teams chat agent built with the [Teams AI SDK](https://github.com/microsoft/teams.ts) and [Claude](https://www.anthropic.com). It holds a normal back-and-forth conversation, and on request — `summarize` — pulls recent messages from the current channel or chat via Microsoft Graph and replies with a structured Adaptive Card (recap plus completed / in-progress / open items).

## How it works

- `src/index.ts` — message handler; routes `summarize` requests to the summary path, everything else to plain chat.
- `src/graph.ts` — fetches recent channel or chat messages via the Teams app's own Graph client.
- `src/llm.ts` — two Claude calls: a plain chat completion, and a forced-tool-call structured summary (via `tool_choice`, since the Anthropic SDK has no `zodOutputFormat` helper).
- `src/card.ts` — renders the structured summary as an Adaptive Card.

## Prerequisites

- Node.js (built against v24.19.0) and npm
- [Teams Developer CLI](https://microsoft.github.io/teams-ai/): `npm i -g @microsoft/teams.cli`
- A Microsoft 365 tenant with sideloading enabled (e.g. the [M365 Developer Program](https://developer.microsoft.com/microsoft-365/dev-program))
- An [Anthropic API key](https://console.anthropic.com/settings/keys)
- [devtunnel](https://learn.microsoft.com/azure/developer/dev-tunnels/get-started) for exposing your local dev server to Teams

## Setup

```sh
npm install
```

Create `.env` from the sample and add your Anthropic key:

```sh
cp .env.sample .env
```

```sh
# .env
ANTHROPIC_API_KEY=your-key-here
```

Log the Teams CLI into your tenant:

```sh
teams login
```

In a separate terminal, host a dev tunnel and leave it running:

```sh
devtunnel host -p 3978 --allow-anonymous
```

Register the bot — this appends `CLIENT_ID`, `CLIENT_SECRET`, and `TENANT_ID` to your `.env` automatically:

```sh
teams app create --endpoint <tunnel-url>/api/messages --name sample-agent --env .env
```

Scope it for personal chat, channels, and group chats:

```sh
teams app update <appId> --scopes personal,team,groupChat
```

## Running

```sh
npm run dev
```

In the terminal from the `teams app create` step, install the app to yourself:

```sh
teams app install <appId>
```

For a channel or group chat, download the install package and sideload it manually (Apps tab → Upload a custom app):

```sh
teams app package download <appId>
```

@mention the bot in a channel/group chat, or just message it 1:1. Try:

```
hey, what can you do?
summarize
```

## Permissions for `summarize`

Reading channel messages needs an RSC permission; reading group chat messages needs a Graph application permission. Neither is granted by default:

```sh
# channels
teams app rsc add <appId> ChannelMessage.Read.Group --type Application
teams app package download <appId>   # re-download after adding a permission
```

For group chats, grant `Chat.Read.All` as a Microsoft Graph **application** permission on the bot's app registration (Entra admin center → App registrations → find by `CLIENT_ID` → API permissions → Add a permission → Microsoft Graph → Application permissions → `Chat.Read.All` → Grant admin consent). After either change, remove and re-add the app in that channel/chat so the refreshed manifest applies.

## Scripts

```sh
npm run dev     # run with hot reload
npm run build   # compile to dist/
npm start        # run the compiled build
```

## License

MIT
