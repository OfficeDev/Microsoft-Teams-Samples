// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
const { TaskModuleUIConstants } = require('../models/taskModuleUIConstants');
const { TaskModuleIds } = require('../models/taskmoduleids');
const { TaskModuleResponseFactory } = require('../models/taskmoduleresponsefactory');

/**
 * TeamsTaskModuleBot handles task module interactions in Microsoft Teams.
 */
class TeamsTaskModuleBot extends TeamsActivityHandler {
    static Actions = [
        TaskModuleUIConstants.AdaptiveCard,
        TaskModuleUIConstants.CustomForm,
        TaskModuleUIConstants.YouTube
    ];

    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;

        // See https://aka.ms/about-bot-activity-message to learn more about the message and other activity types.
        this.onMessage(async (context, next) => {
            // This displays two cards: A HeroCard and an AdaptiveCard. Both have the same
            // options. When any of the options are selected, `handleTeamsTaskModuleFetch`
            // is called.
            const reply = MessageFactory.list([
                this.getTaskModuleHeroCardOptions(),
                this.getTaskModuleAdaptiveCardOptions()
            ]);
            await context.sendActivity(reply);

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    /**
     * Handles task module fetch requests.
     * @param {Object} context - The context object.
     * @param {Object} taskModuleRequest - The task module request object.
     * @returns {Object} - The task module response.
     */
    handleTeamsTaskModuleFetch(context, taskModuleRequest) {
        const cardTaskFetchValue = taskModuleRequest.data.data;
        const taskInfo = {}; // TaskModuleTaskInfo

        if (cardTaskFetchValue === TaskModuleIds.YouTube) {
            // Display the YouTube.html page
            taskInfo.url = taskInfo.fallbackUrl = `${this.baseUrl}/${TaskModuleIds.YouTube}.html`;
            this.setTaskInfo(taskInfo, TaskModuleUIConstants.YouTube);
        } else if (cardTaskFetchValue === TaskModuleIds.CustomForm) {
            // Display the CustomForm.html page, and post the form data back via
            // handleTeamsTaskModuleSubmit.
            taskInfo.url = taskInfo.fallbackUrl = `${this.baseUrl}/${TaskModuleIds.CustomForm}.html`;
            this.setTaskInfo(taskInfo, TaskModuleUIConstants.CustomForm);
        } else if (cardTaskFetchValue === TaskModuleIds.AdaptiveCard) {
            // Display an AdaptiveCard to prompt user for text, and post it back via
            // handleTeamsTaskModuleSubmit.
            taskInfo.card = this.createAdaptiveCardAttachment();
            this.setTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
        }

        return TaskModuleResponseFactory.toTaskModuleResponse(taskInfo);
    }

    /**
     * Handles task module submit requests.
     * @param {Object} context - The context object.
     * @param {Object} taskModuleRequest - The task module request object.
     * @returns {Object} - The task module response.
     */
    async handleTeamsTaskModuleSubmit(context, taskModuleRequest) {
        // Echo the user's input back. In a production bot, this is where you'd add behavior in
        // response to the input.
        await context.sendActivity(MessageFactory.text(`handleTeamsTaskModuleSubmit: ${JSON.stringify(taskModuleRequest.data)}`));

        // Return TaskModuleResponse
        return {
            // TaskModuleMessageResponse
            task: {
                type: 'message',
                value: 'Thanks!'
            }
        };
    }

    /**
     * Sets task info properties.
     * @param {Object} taskInfo - The task info object.
     * @param {Object} uiSettings - The UI settings object.
     */
    setTaskInfo(taskInfo, uiSettings) {
        taskInfo.height = uiSettings.height;
        taskInfo.width = uiSettings.width;
        taskInfo.title = uiSettings.title;
    }

    /**
     * Gets task module HeroCard options.
     * @returns {Object} - The HeroCard options.
     */
    getTaskModuleHeroCardOptions() {
        return CardFactory.heroCard(
            'Dialog (referred as task modules in TeamsJS v1.x) Invocation from Hero Card',
            '',
            null, // No images
            TeamsTaskModuleBot.Actions.map((cardType) => {
                return {
                    type: 'invoke',
                    title: cardType.buttonTitle,
                    value: {
                        type: 'task/fetch',
                        data: cardType.id
                    }
                };
            })
        );
    }

    /**
     * Gets task module AdaptiveCard options.
     * @returns {Object} - The AdaptiveCard options.
     */
    getTaskModuleAdaptiveCardOptions() {
        const adaptiveCard = {
            $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
            version: '1.0',
            type: 'AdaptiveCard',
            body: [
                {
                    type: 'TextBlock',
                    text: 'Dialog (referred as task modules in TeamsJS v1.x) Invocation from Adaptive Card',
                    weight: 'bolder',
                    size: 3
                }
            ],
            actions: TeamsTaskModuleBot.Actions.map((cardType) => {
                return {
                    type: 'Action.Submit',
                    title: cardType.buttonTitle,
                    data: { msteams: { type: 'task/fetch' }, data: cardType.id }
                };
            })
        };

        return CardFactory.adaptiveCard(adaptiveCard);
    }

    /**
     * Creates an AdaptiveCard attachment.
     * @returns {Object} - The AdaptiveCard attachment.
     */
    createAdaptiveCardAttachment() {
        return CardFactory.adaptiveCard({
            version: '1.0.0',
            type: 'AdaptiveCard',
            body: [
                {
                    type: 'TextBlock',
                    text: 'Enter Text Here'
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
        });
    }
}

module.exports.TeamsTaskModuleBot = TeamsTaskModuleBot;
