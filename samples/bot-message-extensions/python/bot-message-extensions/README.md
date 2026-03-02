# Bot Message Extensions - Python

This sample demonstrates a search-based messaging extension in Microsoft Teams that allows users to search for NuGet packages, Wikipedia articles, and find experts directly within the compose area and also in Copilot.

## Prerequisites

- [Python >=3.12, <3.15](https://www.python.org/downloads/)
- pip (recommended) or [uv](https://docs.astral.sh/uv/)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd python/bot-message-extensions
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

Refer to the main [README.md](../../README.md) to interact with your bot in the Microsoft 365 Agents Playground or in Teams.