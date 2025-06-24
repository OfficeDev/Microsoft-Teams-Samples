# Vibe Coding Plan for Bot Conversation Sample

## Step 1: Setup & Validation
- Confirm bot runs locally with current code.
- Validate `.env` and Teams manifest file.

## Step 2: Logging Middleware
- Add middleware for logging all incoming messages.
- Use Winston for logs with levels (info, error).

## Step 3: Error Handling
- Add try/catch blocks to message handling.
- Send fallback message on error.

## Step 4: Refactor Logic
- Move message handling code into `controller/botLogic.js`.
- Make `bot.js` responsible only for routing.

## Step 5: Add Unit Tests
- Add tests covering main conversation flows.
- Use Mocha or Jest.

## Step 6: Add Proactive Messaging
- Support sending messages without user prompt.
 