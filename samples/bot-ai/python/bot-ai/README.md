# Bot AI - Python

This sample demonstrates AI bot message formatting features for Microsoft Teams using Python.

## Prerequisites

- [Python 3.12+](https://www.python.org/downloads/)
- [uv](https://docs.astral.sh/uv/)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd python/bot-ai
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

Refer to the main [README.md](../../README.md) to interact with your bot in the agentsplayground or in Teams.

## Further Reading

- [Microsoft Teams SDK Documentation](https://learn.microsoft.com/microsoftteams/platform/)
- [Bot Framework Documentation](https://docs.botframework.com)
