# Bot AI Messages - Python

This sample demonstrates how to enhance AI-generated bot messages for Microsoft Teams using Python.

## Prerequisites

- [Python](https://www.python.org/downloads/)
- [pip](https://pip.pypa.io/en/stable/installation/)
- [uv](https://docs.astral.sh/uv/)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd python/bot-ai-messages
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

## Further Reading

- [Microsoft Teams SDK Documentation](https://learn.microsoft.com/microsoftteams/platform/)
- [Teams SDK Documentation](https://microsoft.github.io/teams-sdk/welcome)
