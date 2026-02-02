# Bot File Upload - Python

This sample demonstrates a File Upload Bot for Microsoft Teams using Python. The bot can send files to users and receive files from users.

## Prerequisites

- [Python >=3.12,<3.15  a](https://www.python.org/downloads/)
- [uv](https://docs.astral.sh/uv/)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd python/echo_bot
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