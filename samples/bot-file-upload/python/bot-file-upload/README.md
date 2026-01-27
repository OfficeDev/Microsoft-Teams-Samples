# Bot File Upload - Python

This sample demonstrates a File Upload Bot for Microsoft Teams using Python. The bot can send files to users and receive files from users.

## Features

- **Send files to users**: Say "send file" to receive a file from the bot
- **Receive files from users**: Upload files to the bot and it will save them
- **Handle inline images**: Send images directly in chat
- **File consent flow**: Proper consent handling for file uploads

## Prerequisites

- [Python 3.12+](https://www.python.org/downloads/)
- [uv](https://docs.astral.sh/uv/)

## Run the sample

1. Navigate to this directory:

   ```bash
   cd python/bot-file-upload
   ```

2. Create a `.env` file with your credentials:

   ```text
   TENANT_ID=your-tenant-id
   CLIENT_ID=your-client-id
   CLIENT_SECRET=your-client-secret
   ```

3. Install dependencies using uv:

   ```bash
   uv sync
   ```

4. Run the bot:

   ```bash
   uv run main.py
   ```

### Alternative: Using pip

```bash
pip install -e .
python main.py
```

The bot will start listening on `http://localhost:3978`.

## Bot Commands

| Command | Description |
| ------- | ----------- |
| `send file` or `file` | Bot sends a file consent card for downloading a file |
| Send any file | Bot downloads and saves the file |
| Send an image | Bot saves the image and echoes it back |
| Any other message | Bot echoes the message back |

## Further Reading

- [File Upload in Teams](https://learn.microsoft.com/microsoftteams/platform/bots/how-to/bots-filesv4)
- [Microsoft Teams SDK Documentation](https://learn.microsoft.com/microsoftteams/platform/)