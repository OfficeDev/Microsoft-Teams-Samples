# Bot Task Modules - Python

This sample demonstrates Task Modules for Microsoft Teams using Python.

## Prerequisites

- [Python >=3.12](https://www.python.org/downloads/)
- pip (recommended) or [uv](https://docs.astral.sh/uv/)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd python/bot-task-modules
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

Once the bot is running, follow the main [README.md](../../README.md) to provision your app and side-load it into Teams using the [Teams Developer CLI](https://microsoft.github.io/teams-sdk/cli/).