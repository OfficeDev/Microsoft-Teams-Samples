# Prkare Socket Test — APX Default-mode socket bot

A Teams bot that installs into Teams for a **1:1 chat**, but receives everything from **APX** over an
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

1. `POST {APX_BASE_URL}/v3/websockets/connect` (Canary: `https://canary.botapi.skype.com/amer`,
   authenticated with a real **Bot Framework JWT** minted from the bot's client secret) ->
   `{ url, accessToken, expiresIn, protocolVersion }`.
2. Opens a SignalR WebSocket to the negotiated **Azure SignalR** `url` and joins the bot's group.
3. Receives APX->bot frames on the **`Activity`** client method (a `SocketActivityEnvelope`):
   - **invoke** (`type:"invoke"`) -> computes the Bot Framework invoke response and **returns** it as
     the client result (`protocolVersion` + `envelopeId` + `status` + `body`).
   - **one-way activity** (`ackRequired:true`) -> returns a minimal **delivery ack** (status 200);
     for a `message` it also sends a reply via the **HTTP conversation API** (`src/sendActivity.ts`).
4. Keeps **3 sockets** open (`SOCKET_CONNECTIONS`), opened one after the other, each waiting for APX's
   `SocketReady` (connection id) before the next connect. Single-instance.
5. **Keeps them alive** (`src/keepAlive.ts`), the way the Teams SDKs should:
   - **refresh 60 s before each token expires** (`expiresIn` - 60 s): opens the new socket with
     `x-ms-socket-replaces-connection-id: <old connectionId>`, then **drains** the old one (keeps
     answering on it; closes it once nothing is in flight and 5 s have passed since the new socket was
     ready). A 429 or other failure keeps the old socket serving and retries (`Retry-After` on 429);
   - **reconnects** (new connect, no header) when a socket closes without being replaced, backing off
     1 s .. 30 s; stops a slot on 401/403 (socket mode off for the bot);
   - never reconnects for a replaced socket, whether the bot or APX closed it.
6. Runs a **startup self-test** once (`src/selfTest.ts`; `SOCKET_SELF_TEST=false` skips it). Every step
   is logged under the `[flow]` tag with its expected outcome, and a `[summary]` table (expected vs
   actual per step) closes it:
   - **probes APX's per-bot limit**: APX refuses a connect only when the bot already has more than the
     limit open (one spare slot), so with the default limit of 3, connect #4 succeeds and connect #5
     gets **429 + Retry-After**. The extra socket is then closed;
   - **refreshes every socket the old way** (no header): opens the replacement (spare slot), and for the
     first one also tries a second refresh while the old socket is still open (expect 429); the old
     socket is stopped a fixed 5 s after the new one is ready;
   - **refreshes every socket the new way** (`x-ms-socket-replaces-connection-id`): checks the new
     token's `replaces` claim; slots 1 and 3 drain and close the old socket, slot 2 leaves it open so
     APX closes it ~60 s later (APX's reason `Replaced by a newer socket.` is not visible to the JS
     client, only the timing is checked); slot 3 refreshes while slot 2's old socket is still draining;
   - after **every** new socket (self-test and keep-alive), opens a **second socket with the same
     token** and checks APX closes it without `SocketReady` while the first stays open (APX allows one
     socket per token).
7. Logs which socket every delivery arrives on (`route <<`, with a per-socket tally), and compares any
   server close with the token's `exp` (`verdict=CLOSED AT TOKEN EXPIRY`, or `STILL OPEN after token
   expiry`).

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
   - **⛔ Error response** (verb `errorAction`) -> a simulated-error message over the socket.
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
`keepalive` (refresh scheduled / failed, reconnects), `same-token <<`, `refresh <<`, `drain`,
`limit-probe`, `summary`, `route <</>>` (which socket a delivery arrived on), `recv <<` (inbound envelope), `reply >>`
(socket client result), `ack >>` (one-way ack), `reply-card >>/<<` + `http-send >>/<<` (outbound HTTP),
`expiry` (token `exp` reached / still open after it) and `status` (every 5 minutes and at exit). Both
**success and failure** are logged, and each carries the **MS-CV** (from the inbound envelope, or the
APX response headers on outbound HTTP) plus the timestamp — copy them straight into the APX log search.

Each run is also written to `logs/socket-test-<UTC start time>.log` under the repo root, on Windows,
macOS and Linux (gitignored; tokens redacted).



## Prerequisites

- **Node 18+**, VS Code + the **Microsoft 365 Agents Toolkit** extension, an M365 account with
  sideloading enabled.
- The bot's AppId **socket-eligible in APX Canary** (ECS `DeliverEventViaSocketBotAllowList`) and
  known to APX (APS) — the APX-side onboarding.
- APX Canary reachable with the **BotFrontEnd WebSocket connect** deployed
  (`https://canary.botapi.skype.com/amer/v3/websockets/connect`).

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
   - **Start application** (`npm run dev:teamsfx`) — negotiates with APX and opens the socket.

Teams opens in a **single tab** of your normal (signed-in) Edge/Chrome — the bot is debugged via a Node
attach, not a throwaway debug browser, so there is no second sign-in/redirect tab. Install the app and
start a **1:1 chat** with **Prkare Socket Test**.

There is **no dev tunnel and no messaging endpoint** — the bot is reachable only via its outbound
socket to Azure SignalR. To re-seed env files manually (e.g. outside VS Code), run `npm run init:env`.

### Manual run (no Teams UI)

```bash
npm install
# set BOT_ID / BOT_PASSWORD / BOT_TENANT_ID / APX_BASE_URL in .localConfigs (or provision once), then:
npm run build && npm run dev:teamsfx
```

Expected: `prkare-default-mode-socket-bot starting. apx=https://canary.botapi.skype.com/amer botKey=<appId> ...`
then `[socket] conn=conn1 event=ready connectionId=...` for the first 3 sockets, the self-test steps
(`[flow]`, `[limit-probe <<]`, `[refresh <<]`, `[same-token <<]`), the `[summary]` lines, and
`[bot] phase=ready`. From then on `[keepalive]` logs each socket's scheduled refresh.

---

## Configuration

`env/.env.local` (F5) / `.localConfigs` (generated by deploy):

| var | meaning |
|---|---|
| `APX_BASE_URL` | APX connect host + region. Canary: `https://canary.botapi.skype.com/amer`. Local DEBUG APX: `https://localhost:444`. |
| `BOT_ID` | the bot's MSA AppId = socket botKey. On Canary the key is derived from the validated token. |
| `BOT_PASSWORD` | the bot's client secret (provisioned as `SECRET_BOT_PASSWORD`). Required for the authenticated Canary connect. |
| `BOT_TENANT_ID` | the bot's home tenant (single-tenant Entra app) = the Bot Framework token issuer APX validates. Also the tenant put in the connect path when `TENANT_IN_PATH=true`. |
| `TENANT_IN_PATH` | `true` (default) = connect via the tenantized route `{cloud}/{tenantId}/v3/websockets/connect` so APX's `TenantIdInPathFilter` sets `ctx.TenantId` before the socket-eligibility check (a bot token carries no tenant, and a connect has no conversation). Needed for tenant-scoped `DeliverEventViaSocketEnabled` to match. `false` = plain route (use if the ring lacks `TenantIdInPathRoutesEnabled`, or for local DEBUG). |
| `TEAMS_APP_ID` | the Teams app id (provisioned); used by the F5 launch URL. |
| `DEFAULT_DIRECTIVE` | behavior when an invoke/activity carries no `value.directive` (ok/error/delay/drop). |
| `SOCKET_CONNECTIONS` | number of sockets the bot keeps open (default `3`). |
| `SOCKET_SELF_TEST` | `true` (default) runs the startup self-test and the same-token check after every new socket; `false` skips both. |
| `SOCKET_REFRESH_AFTER_SECONDS` | optional, for trying refreshes quickly: refresh each socket this many seconds after it is ready instead of 60 s before its token expires. |
| `NODE_TLS_REJECT_UNAUTHORIZED` | `1` = validate TLS (Canary); `0` = accept a self-signed local APX dev cert. Must be non-empty for the toolkit. |

Secrets (`SECRET_BOT_PASSWORD`) live in `env/.env.local.user` (gitignored, encrypted by the toolkit).

The `SOCKET_*` settings are read from the process environment only (the F5 deploy step doesn't copy them
into `.localConfigs`): set them in your shell or in a root `.env` file (gitignored; see `.env.example`).

---

## Wire contract (kept in sync with APX `Library/Services`)

- Client method **`Activity`**; replies via **client results** (return value), not a separate send.
- Every reply frame carries **`protocolVersion: 1`** (`SocketProtocol.CurrentVersion`) — APX rejects a
  mismatch as `ProtocolMismatch`.
- One-way activities (`ackRequired`) get a status-200 ack; invokes get `{ status, body }`.
- Outbound content sends stay on **HTTP** (`/v3/conversations/{id}/activities`), never the socket.

---

## Notes

- The `messagingEndpoint` in the Bot Framework registration is a placeholder — socket mode never uses
  it. APX delivery uses the socket when the bot is socket-eligible and connected.
- Routing your Teams session's bot traffic to APX **Canary** (flighting) is an environment concern.
