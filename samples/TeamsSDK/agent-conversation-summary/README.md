\# Collab Agent



This sample demonstrates a Teams agent that summarizes conversation history into a structured task breakdown using the Teams SDK, including:



\*\*Conversational Chat\*\* - Handles ordinary back-and-forth messages in personal chats, channels, and group chats



\*\*Conversation Summarization\*\* - A `summarize` request pulls recent messages from the current channel or chat through Microsoft Graph



\*\*Structured Task Extraction\*\* - A forced tool call returns a typed summary split into completed, in-progress, and open items



\*\*Adaptive Card Rendering\*\* - Returns the recap and task breakdown as an Adaptive Card rather than plain text



\*\*Resource-Specific Consent\*\* - Shows the RSC and Graph application permissions required to read channel and group chat messages



\## Table of Contents



\- \[Interaction with Agent](#interaction-with-agent)

\- \[Sample Implementations](#sample-implementations)

\- \[Architecture](#architecture)

\- \[How to run this sample](#how-to-run-this-sample)

&#x20; - \[Prerequisites](#prerequisites)

&#x20; - \[Configure DevTunnels](#configure-devtunnels)

&#x20; - \[Provision with the Teams Developer CLI](#provision-with-the-teams-developer-cli)

&#x20; - \[Run the agent](#run-the-agent)

&#x20; - \[Permissions for summarize](#permissions-for-summarize)

\- \[Teams SDK Troubleshooting](#teams-sdk-troubleshooting)

\- \[Further Reading](#further-reading)



\## Interaction with Agent



\\!\[agent-conversation-summary](agent-conversation-summary.gif)



\## Sample Implementations



| Language | Framework | Directory |

|---|---|---|

| TypeScript | Node.js | \[nodejs](nodejs/agent-conversation-summary) |



\## Architecture



| File | Responsibility |

|---|---|

| `src/index.ts` | Message handler. Routes `summarize` requests to the summary path and everything else to plain chat. |

| `src/graph.ts` | Fetches recent channel or chat messages using the agent's own Graph client. |

| `src/llm.ts` | Two model calls: a plain chat completion, and a forced tool call that returns a structured summary. |

| `src/card.ts` | Renders the structured summary as an Adaptive Card. |



\## How to run this sample



You can run this sample locally in the Teams client after you have provisioned the Teams app, written its credentials into your project's environment file, and started the agent against a public DevTunnels URL.



\### Prerequisites



\- \[Node.js](https://nodejs.org/) (built against v24.19.0) and npm

\- A Microsoft 365 tenant with custom app sideloading enabled — get one through the \[Microsoft 365 Developer Program](https://developer.microsoft.com/microsoft-365/dev-program)

\- The Teams Developer CLI:

&#x20; ```sh

&#x20; npm install -g @microsoft/teams.cli

&#x20; ```

\- The \[DevTunnels CLI](https://learn.microsoft.com/azure/developer/dev-tunnels/get-started)

\- An API key for your model provider



\### Configure DevTunnels



From the `nodejs/agent-conversation-summary` directory, install dependencies:



```sh

npm install

```



Create your environment file and add your model API key:



```sh

cp .env.sample .env

```



Host a tunnel for port 3978 and leave it running in a separate terminal:



```sh

devtunnel host -p 3978 --allow-anonymous

```



Take note of the URL shown after `Connect via browser`.



\### Provision with the Teams Developer CLI



The Teams Developer CLI provisions your Microsoft Entra app, Teams-managed bot registration, and Teams app manifest, and writes the credentials directly into your environment file in a single command.



Sign in with your M365 account:



```sh

teams login

```



Register the agent. This appends `CLIENT\_ID`, `CLIENT\_SECRET`, and `TENANT\_ID` to your `.env` automatically:



```sh

teams app create --name "Collab Agent" --endpoint https://<your-devtunnel-domain>/api/messages --env .env

```



Scope it for personal chat, channels, and group chats:



```sh

teams app update <appId> --scopes personal,team,groupChat

```



\### Run the agent



```sh

npm run dev

```



Install the app to yourself:



```sh

teams app install <appId>

```



For a channel or group chat, download the install package and sideload it manually through \*\*Apps → Upload a custom app\*\*:



```sh

teams app package download <appId>

```



@mention the agent in a channel or group chat, or message it directly. Try:



```

hey, what can you do?

summarize

```



Available scripts:



```sh

npm run dev     # run with hot reload

npm run build   # compile to dist/

npm start       # run the compiled build

```



\### Permissions for summarize



Reading channel messages requires a resource-specific consent (RSC) permission. Reading group chat messages requires a Microsoft Graph application permission. Neither is granted by default.



For channels:



```sh

teams app rsc add <appId> ChannelMessage.Read.Group --type Application

teams app package download <appId>   # re-download after adding a permission

```



For group chats, grant `Chat.Read.All` as a Microsoft Graph application permission on the agent's app registration: \*\*Entra admin center → App registrations → find by `CLIENT\_ID` → API permissions → Add a permission → Microsoft Graph → Application permissions → `Chat.Read.All` → Grant admin consent\*\*.



After either change, remove and re-add the app in that channel or chat so the refreshed manifest applies.



\## Teams SDK Troubleshooting



If you encounter errors you believe exist in the Teams SDK, file an issue on GitHub for the language in which you encountered the issue:



\- \[C#](https://github.com/microsoft/teams.net)

\- \[TypeScript](https://github.com/microsoft/teams.ts)

\- \[Python](https://github.com/microsoft/teams.py)



General issues that exist across all SDK languages can be filed on the \[Teams SDK repository](https://github.com/microsoft/teams-sdk).



\### General Troubleshooting



\- If Teams cannot communicate with your agent, verify your DevTunnels URL is reachable.

\- Ensure your `.env` file is set up correctly and contains all four values.

\- If `summarize` returns no messages, confirm the RSC or Graph permission was granted \*\*and\*\* that you removed and re-added the app afterward.

\- Use the Channels UI in Azure Bot Service in the Azure portal to see detailed endpoint errors.



\## Further Reading



\- \[Microsoft Teams SDK Documentation](https://microsoft.github.io/teams-sdk/)

\- \[Teams Developer CLI](https://microsoft.github.io/teams-sdk/developer-tools/cli/)

\- \[Resource-specific consent in Teams](https://learn.microsoft.com/microsoftteams/platform/graph-api/rsc/resource-specific-consent)

\- \[Adaptive Cards](https://adaptivecards.io/)



\## License



MIT

