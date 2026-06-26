# Bot Auth Quickstart - Python

A Microsoft Teams bot with SSO authentication and Microsoft Graph integration.

## Features

- **SSO Authentication** - Single Sign-On with Microsoft Entra ID
- **Graph Integration** - Fetch user profile via Microsoft Graph


## Prerequisites

- [Python >=3.12, <3.15](https://www.python.org/downloads/)
- pip (recommended) or [uv](https://docs.astral.sh/uv/)
- [Visual Studio Code](https://code.visualstudio.com/) with [Microsoft 365 Agents Toolkit](https://aka.ms/teams-toolkit) extension

## Run the sample

### Using Microsoft 365 Agents Toolkit (Recommended)

1. Open the folder in VS Code
2. Select the Microsoft 365 Agents Toolkit icon on the left toolbar
3. Sign in with your Microsoft 365 account
4. Press `F5` to start debugging

### Using Command Line

1. Navigate to this directory:

```sh
cd python/bot-auth-quickstart
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

## Further Reading

See the [root README](../../README.md) for detailed setup and configuration instructions.
