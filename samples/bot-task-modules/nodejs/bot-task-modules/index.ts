/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

/**
 * Teams Bot Task Module Sample
 * 
 * This bot demonstrates how to use task modules (dialogs) in Microsoft Teams.
 * It shows how to invoke task modules from bot Hero Cards and Adaptive Cards
 * with support for different types of task module content (web pages and cards).
 */

import path from 'path';
import { fileURLToPath } from 'url';
import express from 'express';
import { App } from '@microsoft/teams.apps';
import { 
    Attachment, 
    TaskModuleRequest, 
    TaskModuleResponse,
    UrlTaskModuleTaskInfo,
    CardTaskModuleTaskInfo,
    HeroCard,
    cardAttachment,
    CardAction
} from '@microsoft/teams.api';
import type { IActivityContext } from '@microsoft/teams.apps';
import type { IAdaptiveCard } from '@microsoft/teams.cards';

// Configuration
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const PORT = process.env.PORT || 3978;
const BASE_URL = process.env.BaseUrl || 'http://localhost:3978';

// Task Module Constants
interface UISettings {
    width: number;
    height: number;
    title: string;
    id: string;
    buttonTitle: string;
}

const TaskModuleIds = {
    CustomForm: 'CustomForm',
    AdaptiveCard: 'AdaptiveCard'
} as const;

const TaskModuleUIConstants: Record<string, UISettings> = {
    CustomForm: {
        width: 510,
        height: 450,
        title: 'Custom Form',
        id: 'CustomForm',
        buttonTitle: 'Custom Form'
    },
    AdaptiveCard: {
        width: 400,
        height: 200,
        title: 'Adaptive Card: Inputs',
        id: 'AdaptiveCard',
        buttonTitle: 'Adaptive Card'
    }
};

const Actions = [
    TaskModuleUIConstants.AdaptiveCard,
    TaskModuleUIConstants.CustomForm
];

/**
 * Creates a Hero Card with task module buttons
 */
function createTaskModuleHeroCard(): Attachment {
    const heroCardContent: HeroCard = {
        title: 'Task Module Invocation from Hero Card',
        buttons: Actions.map((action): CardAction => ({
            type: 'invoke',
            title: action.buttonTitle,
            value: {
                type: 'task/fetch',
                data: action.id
            }
        }))
    };
    
    return cardAttachment('hero', heroCardContent);
}

/**
 * Creates an Adaptive Card with task module buttons
 */
function createTaskModuleAdaptiveCard(): Attachment {
    const adaptiveCardContent: IAdaptiveCard = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        version: '1.4',
        type: 'AdaptiveCard',
        body: [
            {
                type: 'TextBlock',
                text: 'Task Module Invocation from Adaptive Card',
                weight: 'Bolder',
                size: 'Large'
            }
        ],
        actions: Actions.map((action) => ({
            type: 'Action.Submit',
            title: action.buttonTitle,
            data: {
                msteams: { type: 'task/fetch' },
                data: action.id
            }
        }))
    };

    return cardAttachment('adaptive', adaptiveCardContent);
}

/**
 * Creates an Adaptive Card for text input in task module
 */
function createInputAdaptiveCard(): Attachment {
    const adaptiveCardContent: IAdaptiveCard = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        version: '1.0',
        type: 'AdaptiveCard',
        body: [
            {
                type: 'TextBlock',
                text: 'Enter Text Here',
                weight: 'Bolder'
            },
            {
                type: 'Input.Text',
                id: 'usertext',
                placeholder: 'add some text and submit',
                isMultiline: true
            }
        ],
        actions: [
            {
                type: 'Action.Submit',
                title: 'Submit'
            }
        ]
    };

    return cardAttachment('adaptive', adaptiveCardContent);
}

/**
 * Creates a task module continue response
 */
function createTaskModuleContinueResponse(taskInfo: UrlTaskModuleTaskInfo | CardTaskModuleTaskInfo): TaskModuleResponse {
    return {
        task: {
            type: 'continue',
            value: taskInfo
        }
    };
}

/**
 * Creates a task module message response
 */
function createTaskModuleMessageResponse(message: string): TaskModuleResponse {
    return {
        task: {
            type: 'message',
            value: message
        }
    };
}

// Teams App Setup
const app = new App({
    clientId: process.env.CLIENT_ID,
    clientSecret: process.env.CLIENT_SECRET,
    tenantId: process.env.TENANT_ID
});

/**
 * Handle incoming messages - Display task module cards
 */
app.message(/.*/i, async (context: IActivityContext<any>) => {
    const heroCard = createTaskModuleHeroCard();
    const adaptiveCard = createTaskModuleAdaptiveCard();
    
    await context.send({
        type: 'message',
        attachments: [heroCard, adaptiveCard],
        attachmentLayout: 'list'
    });
});

/**
 * Handle dialog.open - Show task module (equivalent to @app.on_dialog_open in Python)
 */
app.on('dialog.open', async (context: IActivityContext<any>) => {
    const taskModuleRequest = context.activity.value as TaskModuleRequest;
    let cardData = taskModuleRequest.data?.data;
    
    // Default to AdaptiveCard if no data provided
    if (!cardData) {
        cardData = TaskModuleIds.AdaptiveCard;
    }
    
    let taskInfo: UrlTaskModuleTaskInfo | CardTaskModuleTaskInfo;
    
    if (cardData === TaskModuleIds.CustomForm) {
        taskInfo = {
            title: TaskModuleUIConstants.CustomForm.title,
            width: TaskModuleUIConstants.CustomForm.width,
            height: TaskModuleUIConstants.CustomForm.height,
            url: `${BASE_URL}/CustomForm.html`,
            fallbackUrl: `${BASE_URL}/CustomForm.html`
        };
    } else {
        taskInfo = {
            title: TaskModuleUIConstants.AdaptiveCard.title,
            width: TaskModuleUIConstants.AdaptiveCard.width,
            height: TaskModuleUIConstants.AdaptiveCard.height,
            card: createInputAdaptiveCard()
        };
    }
    
    return createTaskModuleContinueResponse(taskInfo);
});

/**
 * Handle dialog.submit - Process task module submission (equivalent to @app.on_dialog_submit in Python)
 */
app.on('dialog.submit', async (context: IActivityContext<any>) => {
    const taskModuleRequest = context.activity.value as TaskModuleRequest;
    const submittedData = taskModuleRequest.data || {};
    
    // Build a formatted Adaptive Card to display the submitted data
    const bodyItems: any[] = [
        {
            type: 'TextBlock',
            text: 'Task Module Submission Received',
            size: 'Large',
            weight: 'Bolder'
        }
    ];
    
    // Add each field from the submitted data
    if (submittedData && Object.keys(submittedData).length > 0) {
        for (const [key, value] of Object.entries(submittedData)) {
            // Format the key nicely (capitalize, replace underscores with spaces)
            const formattedKey = key.replace(/_/g, ' ').replace(/\b\w/g, (l) => l.toUpperCase());
            bodyItems.push({
                type: 'TextBlock',
                text: `**${formattedKey}:** ${value}`,
                wrap: true
            });
        }
    } else {
        bodyItems.push({
            type: 'TextBlock',
            text: 'No data submitted',
            isSubtle: true
        });
    }
    
    const resultCard: IAdaptiveCard = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        version: '1.4',
        type: 'AdaptiveCard',
        body: bodyItems
    };
    
    // Send the formatted card
    await context.send({
        type: 'message',
        text: 'Task module submission received',
        attachments: [cardAttachment('adaptive', resultCard)]
    });
    
    // Return a message response
    return createTaskModuleMessageResponse('Thanks!');
});

/**
 * Serve app configuration endpoint (used by task module web pages)
 */
app.http.get('/getAppConfig', async (_req: any, res: any) => {
    res.json({
        MicrosoftAppId: process.env.CLIENT_ID
    });
});

/**
 * Serve static HTML pages for task modules (CustomForm.html)
 */
const pagesPath = path.join(__dirname, 'pages');
app.http.use(express.static(pagesPath));


// Error Handling
app.event('error', async (event: any) => {
    console.error('[Error]', event.error);
    if (event.error instanceof Error) {
        console.error(event.error.stack);
    }
});

// Start Server
await app.start(PORT);