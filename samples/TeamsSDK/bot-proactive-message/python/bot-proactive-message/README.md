# Bot Proactive Message - Python

This sample demonstrates how to send proactive messages to Microsoft Teams users using Python.

## Prerequisites

- [Python >=3.12, <3.15](https://www.python.org/downloads/)
- pip (recommended) or [uv](https://docs.astral.sh/uv/))

## Run the sample

1. Navigate to this directory:
   ```bash
   cd python/bot-proactive-message
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

Refer to the main [README.md](../../README.md) to interact with your bot in Teams.
