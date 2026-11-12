# Socket Test — ACS Default-mode socket bot

A Teams bot that installs into Teams for a **1:1 chat**, but receives everything from **ACS** over an
**Azure SignalR Default-mode socket** — so it exposes **no public messaging endpoint and no dev
tunnel**. It accepts invokes and one-way activities over the socket and responds there (SignalR
**client results**), and sends any outbound content over the existing **HTTP conversation API**, as
the socket-mode design intends.

Set up like a normal M365 Agents Toolkit bot: **F5 does all the registration** (Teams app + Entra app
+ Bot Framework) and opens Teams in the browser to install the app — minus the tunnel/endpoint steps.
The gitignored env files are **auto-seeded from their `.sample` templates on first run**, so there is
no manual copy step.

---

## What it does

1. `POST https://{region}.botapi.skype.com/{region}/v3/websockets/connect` for each region (`amer`, `emea`,
   `apac` by default), authenticated with a real **Bot Framework JWT** minted from the bot's client secret ->
   `{ url, accessToken, expiresIn, protocolVersion }`. Each region's own host always reaches that region's front
   end, so the socket lands on the SignalR resource that delivers the region's traffic. Only connect calls use the
   per-region host; every other call (replies) goes to the `serviceUrl` of the incoming activity.
2. Opens a SignalR WebSocket to the negotiated **Azure SignalR** `url` and joins the bot's group.
3. Receives ACS->bot frames on the **`Activity`** client method (a `SocketActivityEnvelope`):
   - **invoke** (`type:"invoke"`) -> computes the Bot Framework invoke response and **returns** it as
     the client result (`protocolVersion` + `envelopeId` + `status` + `body`).
   - **one-way activity** (`ackRequired:true`) -> returns a minimal **delivery ack** (status 200);
     for a `message` it also sends a reply via the **HTTP conversation API** (`src/sendActivity.ts`).
4. Keeps **one socket per region** open (`ACS_REGIONS`, default `amer,emea,apac`), all opened at startup,
   each waiting for ACS's `SocketReady` (connection id). Each region delivers only its own traffic, so the
   bot connects in every region; a problem in one region doesn't affect the others. Single-instance.
5. **Keeps them alive** (`src/keepAlive.ts`), the way the Teams SDKs should:
   - **refresh 60 s before each token expires** (`expiresIn` - 60 s): opens the new socket with
     `x-ms-socket-replaces-connection-id: <old connectionId>`, then **drains** the old one (keeps
     answering on it; closes it once nothing is in flight and 5 s have passed since the new socket was
     ready). A 429 or other failure keeps the old socket serving and retries (`Retry-After` on 429);
   - **reconnects** (new connect, no header) when a socket closes without being replaced, backing off
     1 s .. 30 s; stops a region on 401/403 (socket mode off for the bot);
   - never reconnects for a replaced socket, whether the bot or ACS closed it.
6. Logs which socket and region every delivery arrives on (`route <<`, with a per-socket tally), and
   compares any server close with the token's `exp` (`verdict=CLOSED AT TOKEN EXPIRY`, or `STILL OPEN
   after token expiry`).

---

## End-to-end demo flow

The bot is wired to make the whole socket round-trip visible in the 1:1 chat:

1. **Send any message** (e.g. `hi`). It arrives as a one-way activity **over the socket**; the bot acks
   over the socket and replies with an **adaptive card over HTTP** (`invokeDemoCard`, `src/cards.ts`).
2. **Click a card button.** Each button fires an **invoke back to the bot over the socket**, and the bot
   responds **over the socket** (client result):
   - **🔄 Refresh card** (`Action.Execute` verb `refreshCard`) -> `adaptiveCard/action` invoke -> the bot
     returns an `AdaptiveCardInvokeResponse`; Teams re-renders the card **in place** (with a live click
     counter + timestamp), so you can see the socket response land.
   - **💬 Show message** (verb `showMessage`) -> a message-type invoke response (toast).
   - **⛔ Error response** (verb `errorAction`) -> a real Universal Actions error over the socket
     (`statusCode: 400`, `type: application/vnd.microsoft.error`, `value: { code, message }`), which Teams
     shows as an error on the card.
   - **🗔 Open dialog** (`Action.Submit` -> `task/fetch`) -> the bot returns a **task module** over the
     socket; submitting it fires `task/submit` (also over the socket).

Message replies stay on **HTTP**; invoke responses stay on the **socket** — exactly the socket-mode split.

---

## Logging (correlation)

Every call in/out of the bot logs one greppable line via `src/log.ts`:

```
[<ISO-8601 UTC>] [<tag>] key=value …  MS-CV=<cv>
```

Tags: `negotiate >>/<<` (connect: status, `expiresIn`, `protocolVersion`, SignalR URL, token length and
`iat`/`exp` claims, `tokenId`, the `replaces` header and claim, `Retry-After` on failure),
`SocketReady <<`, `socket` (connected/ready/closed with the close error and verdict), `signalr` (the
SignalR client's own log), `bot` / `flow` (start-up and each step with its expected outcome),
`keepalive` (refresh scheduled / failed, reconnects), `refresh <<`, `drain`,
`route <</>>` (which socket a delivery arrived on), `recv <<` (inbound envelope), `reply >>`
(socket client result), `ack >>` (one-way ack), `reply-card >>/<<` + `http-send >>/<<` (outbound HTTP),
`expiry` (token `exp` reached / still open after it) and `status` (every 5 minutes and at exit). Both
**success and failure** are logged, and each carries the **MS-CV** (from the inbound envelope, or the
ACS response headers on outbound HTTP) plus the timestamp — copy them straight into the ACS log search.

Each run is also written to `logs/socket-test-<UTC start time>.log` under the repo root, on Windows,
macOS and Linux (gitignored; tokens redacted).



## Prerequisites

- **Node 18+**, VS Code + the **Microsoft 365 Agents Toolkit** extension, an M365 account with
  sideloading enabled.
- The bot's AppId **socket-eligible in ACS** (socket mode turned on for the bot) and known to ACS
  (APS) — the ACS-side onboarding.
- The ACS **WebSocket connect** endpoint reachable in each region
  (`https://{amer|emea|apac}.botapi.skype.com/{amer|emea|apac}/v3/websockets/connect`).

---

## Run it (two steps)

1. **Sign in** to the **Microsoft 365 Agents Toolkit** extension with the test account you want the bot
   to run as (sideloading enabled).
2. Press **F5** and pick **Debug in Teams (Edge)** or **(Chrome)**.

That's it. The `Start socket bot locally` task chain does the rest:
   - **Free debug port** (9239) and **Validate prerequisites** (Node, M365 sign-in),
   - **npm install**,
   - **Initialize env files (if missing)** — seeds `env/.env.local` and `env/.env.local.user` from
     their `.sample` templates (with defaults like `TEAMSFX_ENV=local` prepopulated). **Existing files
     are never overwritten.**
   - **Provision** (`teamsapp provision --env local`) — creates the Teams app, the Entra app
     (client id = bot id / socket botKey, + client secret), and the Bot Framework registration
     (**placeholder** messaging endpoint — never called in socket mode),
   - **Deploy** — writes runtime env to `.localConfigs`,
   - **Start application** (`npm run dev:teamsfx`) — negotiates with ACS and opens the socket.

Teams opens in a **single tab** of your normal (signed-in) Edge/Chrome — the bot is debugged via a Node
attach, not a throwaway debug browser, so there is no second sign-in/redirect tab. Install the app and
start a **1:1 chat** with **Socket Test**.

There is **no dev tunnel and no messaging endpoint** — the bot is reachable only via its outbound
socket to Azure SignalR. To re-seed env files manually (e.g. outside VS Code), run `npm run init:env`.

### Manual run (no Teams UI)

```bash
npm install
# set BOT_ID / BOT_PASSWORD / BOT_TENANT_ID (and optionally ACS_REGIONS) in .localConfigs (or provision once), then:
npm run build && npm run dev:teamsfx
```

Expected: `default-mode-socket-bot starting. connect=https://{region}.botapi.skype.com regions=amer,emea,apac botKey=<appId> ...`
then `[socket] conn=connN region=<region> event=ready connectionId=...` for one socket in each region, and
`[bot] phase=ready`. From then on `[keepalive]` logs each socket's scheduled refresh (60 s before its token
expires).

---

## Configuration

`env/.env.local` (F5) / `.localConfigs` (generated by deploy):

| var | meaning |
|---|---|
| `ACS_REGIONS` | regions to connect in, one socket each (comma-separated; default `amer,emea,apac`; `eudb` is also supported, other values are ignored). Each region connects at `https://{region}.botapi.skype.com/{region}/v3/websockets/connect`; the connect host isn't configurable. |
| `BOT_ID` | the bot's MSA AppId = socket botKey. ACS derives the key from the validated token. |
| `BOT_PASSWORD` | the bot's client secret (provisioned as `SECRET_BOT_PASSWORD`). Required for the authenticated connect. |
| `BOT_TENANT_ID` | the bot's home tenant (single-tenant Entra app) = the Bot Framework token issuer ACS validates. Also the tenant put in the connect path when `TENANT_IN_PATH=true`. |
| `TENANT_IN_PATH` | `false` (default) = plain route `{region}/v3/websockets/connect`. `true` = tenantized route `{region}/{tenantId}/v3/websockets/connect`, which needs `TenantIdInPathRoutesEnabled` in the ring. |
| `TEAMS_APP_ID` | the Teams app id (provisioned); used by the F5 launch URL. |
| `DEFAULT_DIRECTIVE` | behavior when an invoke/activity carries no `value.directive` (ok/error/delay/drop). |
| `NODE_TLS_REJECT_UNAUTHORIZED` | keep `1` (validate TLS). Must be non-empty for the toolkit. |

Secrets (`SECRET_BOT_PASSWORD`) live in `env/.env.local.user` (gitignored, encrypted by the toolkit).

`ACS_REGIONS` is read from the process environment only (the F5 deploy step doesn't copy it into
`.localConfigs`): set it in your shell or in a root `.env` file (gitignored; see `.env.example`).

---

## Wire contract (kept in sync with ACS `Library/Services`)

- Client method **`Activity`**; replies via **client results** (return value), not a separate send.
- Every reply frame carries **`protocolVersion: 1`** (`SocketProtocol.CurrentVersion`) — ACS rejects a
  mismatch as `ProtocolMismatch`.
- One-way activities (`ackRequired`) get a status-200 ack; invokes get `{ status, body }`.
- Outbound content sends stay on **HTTP** (`/v3/conversations/{id}/activities`), never the socket.

---

## Notes

- The `messagingEndpoint` in the Bot Framework registration is a placeholder — socket mode never uses
  it. ACS delivery uses the socket when the bot is socket-eligible and connected.
