# Targeted Messages in Microsoft Teams - TypeScript

This sample demonstrates how to use **targeted messaging** in Microsoft Teams. Targeted messages are private messages that appear in a shared channel or group chat but are **only visible to a specific user**. The sample implements a reminder bot where all bot responses — confirmations, reminder deliveries, active reminder lists, and snooze confirmations — are sent as targeted messages.

## Included Features

- Bots
- Targeted Messaging (`MessageActivity.withRecipient()`)
- Proactive Targeted Messaging (`app.send()`)
- Adaptive Cards with `Action.Execute`
- Mention Stripping (`stripMentionsText()`)

## Prerequisites

- [Node.js](https://nodejs.org/) (LTS version recommended)

## Run the sample

1. Navigate to this directory:
   ```
   cd targeted-messages/bot-targeted-messages/nodejs
   ```

2. Install dependencies:
   ```
   npm install
   ```

3. Run the bot:
   ```
   npm start
   ```

The bot will start listening on `http://localhost:3978`.

Refer to the main [README.md](../../README.md) to interact with your bot in the agentsplayground or in Teams.

## Running the sample

Once installed, you can use the following commands in any **channel** or **group chat**:

### Set a Reminder

- `remind me in 5 minutes to check email`
- `remind me in 1 hour meeting starts`
- `remind me in 30 seconds test`
- `remind @John in 10 minutes review PR`

### Supported Time Formats

- Seconds: `30 seconds`, `30 secs`, `30s`
- Minutes: `5 minutes`, `5 mins`, `5m`
- Hours: `1 hour`, `2 hrs`, `1h`

### Manage Reminders

- `my-reminders` — View your active reminders (targeted, only you see the list)
- `cancel-reminder [id]` — Cancel a specific reminder
- `reminder-help` — Show help information

### Where Targeted Messages Are Used

Every bot response in this sample is a targeted message — only the intended recipient can see it:

| Action | Recipient | How |
|---|---|---|
| Setting a reminder | The command sender | `send(new MessageActivity(...).withRecipient(creator, true))` |
| Reminder delivery | The reminder target | `app.send(conversationId, new MessageActivity(...).withRecipient(recipient, true))` |
| Viewing active reminders | The command sender | `send(new MessageActivity(...).withRecipient(sender, true))` |
| Snooze confirmation | The snoozing user | `send(new MessageActivity(...).withRecipient(recipient, true))` |

### How Targeted Messaging Works

The bot uses the Teams SDK (`@microsoft/teams.apps` / `@microsoft/teams.api`) to send targeted messages:

- **`MessageActivity.withRecipient(account, true)`** — marks the message as targeted for a specific user. The second parameter (`true`) sets `isTargeted`.
- **`app.send(conversationId, activity)`** — sends a proactive targeted message (e.g., delivering a reminder after a delay).
- **`stripMentionsText(activity, { accountId })`** — strips bot or user @mentions from incoming message text.
- **Adaptive Cards** with `Action.Execute` — provide interactive buttons (Cancel, Dismiss, Snooze) on targeted cards.

The message appears in the shared conversation thread but is **only visible to the specified recipient**.

## Further reading

- [Targeted Messages in Teams](https://microsoft.github.io/teams-sdk/typescript/essentials/sending-messages/#targeted-messages)
- [Send and receive messages with a bot](https://learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-messages)
- [Adaptive Cards](https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#adaptive-card)
- [Microsoft Teams SDK for Node.js](https://www.npmjs.com/package/@microsoft/teams.apps)
