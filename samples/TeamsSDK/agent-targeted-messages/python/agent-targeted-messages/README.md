# Targeted Messages in Microsoft Teams - Python

This sample demonstrates how to use **targeted messaging** in Microsoft Teams. Targeted messages are private messages that appear in a channel or group chat but are **only visible to a specific user**. The sample implements a reminder bot where all bot responses — confirmations, reminder deliveries, active reminder lists, and snooze confirmations — are sent as targeted messages.

## Prerequisites

- [Python >=3.12, <3.15](https://www.python.org/downloads/)
- pip (recommended) or [uv](https://docs.astral.sh/uv/)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd samples/TeamsSDK/agent-targeted-messages/python/agent-targeted-messages
   ```
2. Run the agent using pip:

```bash
pip install -e .
python main.py
```

### Alternative: Using uv

1. Install dependencies using uv:
   ```bash
   uv sync
   ```

2. Run the agent:
   ```bash
   uv run main.py
   ```

The agent will start listening on `http://localhost:3978`.

Refer to the main [README.md](../../../README.md) to interact with your agent in the agentsplayground or in Teams.