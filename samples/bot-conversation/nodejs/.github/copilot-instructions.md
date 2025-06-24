# GitHub Copilot Agent Instructions - Bot Conversation Sample (Node.js)

## Project Overview
This sample demonstrates a Microsoft Teams bot built with Node.js and Bot Framework SDK. It handles conversation flow and messaging with Teams users.

## Coding Guidelines
- Use ES6+ JavaScript (prefer `const` and `let`).
- Async/await syntax for asynchronous operations.
- Modular architecture: separate concerns in different files.
- Use JSDoc comments for all public functions.
- Write unit tests with Mocha or Jest.

## Tools & Scripts
- Node.js v18+
- Run `npm install` to install dependencies.
- Start bot with `npm start`.
- Run lint with `npm run lint`.
- Run tests with `npm test`.

## Development Tasks Examples
- Add error handling to `onMessage` handler.
- Refactor bot logic into controller class.
- Add proactive messaging support.
- Improve logging and telemetry.

## Testing Requirements
- Every new feature must include unit tests.
- Tests should be run and pass before merging.

## Additional Notes
- Use environment variables for secrets.
- Follow existing naming conventions (camelCase for vars).