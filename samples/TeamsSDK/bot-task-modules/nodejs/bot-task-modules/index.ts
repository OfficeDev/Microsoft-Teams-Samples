// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    cardAttachment,
} from '@microsoft/teams.api';
import type { IActivityContext } from '@microsoft/teams.apps';
import {
    AdaptiveCard,
    TextBlock,
    TextInput,
    SubmitAction,
    TaskFetchAction,
} from '@microsoft/teams.cards';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const PORT = process.env.PORT || 3978;
const BOT_ENDPOINT = process.env.BOT_ENDPOINT || 'http://localhost:3978';

if (!process.env.BOT_ENDPOINT) {
    console.log('No remote endpoint detected. Using webpages will not work as expected');
}

/** Returns an Adaptive Card attachment with three task/fetch buttons to open each task module type. */
function createTaskModuleAdaptiveCard(): Attachment {
    const card = new AdaptiveCard(
        new TextBlock('Task Module Invocation from Adaptive Card', { weight: 'Bolder', size: 'Large' }),
    ).withVersion('1.4').withActions(
        new TaskFetchAction({ data: 'AdaptiveCard' }).withTitle('Adaptive Card'),
        new TaskFetchAction({ data: 'CustomForm' }).withTitle('Custom Form'),
        new TaskFetchAction({ data: 'MultiStep' }).withTitle('Multi-step Form'),
    );
    return cardAttachment('adaptive', card);
}

/** Returns an Adaptive Card attachment with a multiline text input field used inside the AdaptiveCard task module. */
function createTextInputCard(): Attachment {
    const card = new AdaptiveCard(
        new TextBlock('Enter Text Here', { weight: 'Bolder' }),
        new TextInput({ id: 'usertext', placeholder: 'add some text and submit', isMultiline: true }),
    ).withVersion('1.0').withActions(
        new SubmitAction({ title: 'Submit' }),
    );
    return cardAttachment('adaptive', card);
}

/** Returns step 1 of the multi-step form card, collecting the user's name. */
function createMultiStepStep1Card(): Attachment {
    const card = new AdaptiveCard(
        new TextBlock('Step 1 of 2 - Your Name', { size: 'Large', weight: 'Bolder' }),
        new TextInput({ id: 'name', label: 'Name', placeholder: 'Enter your name', isRequired: true }),
    ).withVersion('1.4').withActions(
        new SubmitAction({ title: 'Next', data: { submissiontype: 'multi_step_1' } }),
    );
    return cardAttachment('adaptive', card);
}

/** Returns step 2 of the multi-step form card, collecting the user's email and carrying the name from step 1. */
function createMultiStepStep2Card(name: string): Attachment {
    const card = new AdaptiveCard(
        new TextBlock('Step 2 of 2 - Your Email', { size: 'Large', weight: 'Bolder' }),
        new TextInput({ id: 'email', label: 'Email', placeholder: 'Enter your email', isRequired: true }),
    ).withVersion('1.4').withActions(
        new SubmitAction({ title: 'Submit', data: { submissiontype: 'multi_step_2', name } }),
    );
    return cardAttachment('adaptive', card);
}

const app = new App({
    clientId: process.env.CLIENT_ID,
    clientSecret: process.env.CLIENT_SECRET,
});

/** Handles any incoming message by sending the adaptive card with task module launch buttons. */
app.message(/.*/i, async (context: IActivityContext<any>) => {
    await context.send({
        type: 'message',
        attachments: [createTaskModuleAdaptiveCard()],
    });
});

/** Handles task/fetch invocations by returning the appropriate task module (URL page, adaptive card, or multi-step card) based on the requested type. */
app.on('dialog.open', async (context: IActivityContext<any>) => {
    const taskModuleRequest = context.activity.value as TaskModuleRequest;
    const cardData = taskModuleRequest.data?.data ?? 'AdaptiveCard';

    if (cardData === 'CustomForm') {
        const taskInfo: UrlTaskModuleTaskInfo = {
            title: 'Custom Form',
            width: 510,
            height: 450,
            url: `${BOT_ENDPOINT}/CustomForm/`,
            fallbackUrl: `${BOT_ENDPOINT}/CustomForm/`,
        };
        return { task: { type: 'continue', value: taskInfo } } as TaskModuleResponse;
    }

    if (cardData === 'MultiStep') {
        const taskInfo: CardTaskModuleTaskInfo = {
            title: 'Multi-step Form',
            width: 400,
            height: 300,
            card: createMultiStepStep1Card(),
        };
        return { task: { type: 'continue', value: taskInfo } } as TaskModuleResponse;
    }

    // Default: AdaptiveCard
    const taskInfo: CardTaskModuleTaskInfo = {
        title: 'Adaptive Card: Inputs',
        width: 400,
        height: 200,
        card: createTextInputCard(),
    };
    return { task: { type: 'continue', value: taskInfo } } as TaskModuleResponse;
});

/** Handles task/submit invocations by routing on submissiontype: advances multi-step flow, confirms custom form, or echoes adaptive card text input. */
app.on('dialog.submit', async (context: IActivityContext<any>) => {
    const taskModuleRequest = context.activity.value as TaskModuleRequest;
    const data = taskModuleRequest.data || {};
    const submissionType = typeof data === 'object' ? data.submissiontype : undefined;

    if (submissionType === 'multi_step_1') {
        const taskInfo: CardTaskModuleTaskInfo = {
            title: 'Multi-step Form: Step 2',
            width: 400,
            height: 300,
            card: createMultiStepStep2Card(data.name),
        };
        return { task: { type: 'continue', value: taskInfo } } as TaskModuleResponse;
    }

    if (submissionType === 'multi_step_2') {
        const { name, email } = data;
        await context.send(`Hi ${name}, thanks for submitting! Your email is ${email}`);
        return { task: { type: 'message', value: 'Multi-step form completed!' } } as TaskModuleResponse;
    }

    if (submissionType === 'custom_form') {
        const { name, email } = data;
        await context.send(`Hi ${name}, thanks for submitting! Your email is ${email}`);
        return { task: { type: 'message', value: 'Form submitted successfully' } } as TaskModuleResponse;
    }

    // Default: adaptive card text input
    const usertext = data?.usertext;
    await context.send(`You submitted: ${usertext}`);
    return { task: { type: 'message', value: 'Thanks for submitting!' } } as TaskModuleResponse;
});

/** Logs any unhandled errors raised by the app framework to the console. */
app.event('error', async (event: any) => {
    console.log(`Error occurred: ${event.error}`);
    if (event.context) {
        console.log(`Context: ${event.context}`);
    }
});

const pagesPath = path.join(__dirname, 'pages');
app.http.use(express.static(pagesPath));

await app.start(PORT);