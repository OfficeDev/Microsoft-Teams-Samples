# Cards and Attachments Bot - Python

This sample demonstrates how to upload files in Microsoft Teams using a bot built with Teams SDK.

## Prerequisites

- [Python >=3.12,<3.15](https://www.python.org/downloads/)
- [pip](https://pip.pypa.io/en/stable/installation/)
- [uv](https://docs.astral.sh/uv/)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd python/bot-cards
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

Refer to the main [README.md](../../README.md) to interact with your bot in the agentsplayground or in Teams.