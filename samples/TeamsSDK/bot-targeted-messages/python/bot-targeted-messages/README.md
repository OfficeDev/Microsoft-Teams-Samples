# Targeted Messages in Microsoft Teams - Python

This sample demonstrates how to use **targeted messaging** in Microsoft Teams. Targeted messages are private messages that appear in a shared channel or group chat but are **only visible to a specific user**. The sample implements a reminder bot where all bot responses — confirmations, reminder deliveries, active reminder lists, and snooze confirmations — are sent as targeted messages.

## Included Features

- Bots
- Targeted Messaging (`MessageActivityInput.with_recipient()` with `isTargeted=True`)
- Proactive Targeted Messaging (`app.send()`)
- Adaptive Cards with `Action.Execute`
- Mention Stripping (`strip_mentions_text()`)

## Prerequisites

- [Python >=3.12, <3.15](https://www.python.org/downloads/)
- pip (recommended) or [uv](https://docs.astral.sh/uv/)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd samples/TeamsSDK/bot-targeted-messages/python/bot-targeted-messages
   ```
2. Run the bot using pip:

```bash
pip install -e .
python main.py
```

### Alternative: Using uv

1. Install dependencies using uv:
   ```bash
   uv sync
   ```

2. Run the bot:
   ```bash
   uv run main.py
   ```

The bot will start listening on `http://localhost:3978`.

Refer to the main [README.md](../../../README.md) to interact with your bot in the agentsplayground or in Teams.

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
| Setting a reminder | The command sender | `ctx.send(MessageActivityInput(...).add_card(card).with_recipient(creator))` |
| Reminder delivery | The reminder target | `app.send(conversation_id, MessageActivityInput(...).add_card(card).with_recipient(recipient))` |
| Viewing active reminders | The command sender | `ctx.send(MessageActivityInput(...).with_recipient(sender))` |
| Snooze confirmation | The snoozing user | `ctx.send(MessageActivityInput(...).add_card(card).with_recipient(recipient))` |

### How Targeted Messaging Works

The bot uses the Teams SDK (`microsoft-teams-apps` / `microsoft-teams-api`) to send targeted messages:

- **`Account(id=..., name=..., role='user', isTargeted=True)`** — creates a recipient account marked as targeted.
- **`MessageActivityInput.with_recipient(account)`** — sets the targeted recipient on the message.
- **`app.send(conversation_id, activity)`** — sends a proactive targeted message (e.g., delivering a reminder after a delay).
- **`strip_mentions_text(activity, StripMentionsTextOptions(account_id=...))`** — strips bot or user @mentions from incoming message text.
- **Adaptive Cards** with `Action.Execute` — provide interactive buttons (Cancel, Dismiss, Snooze) on targeted cards.

The message appears in the shared conversation thread but is **only visible to the specified recipient**.

## Further reading

- [Targeted Messages in Teams](https://learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/send-proactive-messages)
- [Send and receive messages with a bot](https://learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-messages)
- [Adaptive Cards](https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#adaptive-card)
- [Microsoft Teams SDK for Python](https://pypi.org/project/microsoft-teams-apps/)