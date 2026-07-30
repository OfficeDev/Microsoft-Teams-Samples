// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import path from 'path';
import { CloudAdapter, ConfigurationBotFrameworkAuthentication } from 'botbuilder';
import { BotActivityHandler } from '../bot/botActivityHandler';
import type { Request, Response } from 'express';

// Load environment variables from .env file
const ENV_FILE = path.join(__dirname, '../../.env');
require('dotenv').config({ path: ENV_FILE });

// Initialize bot framework authentication using environment variables
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env as Record<string, string>);

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Handle errors during bot turn processing
adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );
};

// Create bot handlers
const botActivityHandler = new BotActivityHandler();

// This function is the entry point for every incoming request from Teams/Emulator
const botHandler = async (req: Request, res: Response) => {
    await adapter.process(req, res, async (context) => {
        await botActivityHandler.run(context);
    });
};

export default botHandler;
