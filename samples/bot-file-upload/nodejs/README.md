# File Upload Bot - Node.js (TypeScript)

This sample demonstrates a File Upload Bot for Microsoft Teams using Node.js and TypeScript. The bot can send files to users using file consent cards and receive files from users.

## Prerequisites

- [Node.js](https://nodejs.org/) (LTS version recommended)

## Run the sample

1. Navigate to this directory:
   ```bash
   cd nodejs/bot-file-upload
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Run the bot:
   ```bash
   npm start
   ```

The bot will start listening on `http://localhost:3978`.

## Features

- **Send Files to Users**: The bot sends a file consent card requesting permission to upload files to user's OneDrive
- **Receive Files from Users**: The bot accepts files sent by users and saves them locally
- **Handle Inline Images**: The bot processes inline images and other attachments

## Configuration

`.env` fill in your credentials:

```
TENANT_ID=your-tenant-id
CLIENT_ID=your-client-id
CLIENT_SECRET=your-client-secret
```

Refer to the main [README.md](../../README.md) to interact with your bot in the agentsplayground or in Teams.
