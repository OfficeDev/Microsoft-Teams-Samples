# Bot Message Reaction - Python

This sample demonstrates a Message Reaction Bot for Microsoft Teams using Python. The bot echoes back messages and responds to message reactions (likes, emojis, etc.).

## Prerequisites

- [Python 3.12+](https://www.python.org/downloads/)
- [uv](https://docs.astral.sh/uv/)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd python/bot-message-reaction
   ```

2. Install dependencies using uv:
   ```bash
   uv sync
   ```

3. Run the bot:
   ```bash
   uv run main.py
   ```

### Alternative: Using pip

```bash
pip install -e .
python main.py
```

The bot will start listening on `http://localhost:3978`.

## Features

- **Echo Messages**: The bot echoes back any message sent to it
- **Reaction Tracking**: When you react to a bot message with an emoji, the bot will acknowledge the reaction
- **Reaction Removal**: When you remove a reaction, the bot will acknowledge the removal

Refer to the main [README.md](../../README.md) to interact with your bot in the agentsplayground or in Teams.
