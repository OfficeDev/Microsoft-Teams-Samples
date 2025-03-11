// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ComponentDialog, WaterfallDialog, TextPrompt } = require('botbuilder-dialogs');
const { CardFactory, MessageFactory, InputHints } = require('botbuilder');

const USERNAMEPASSWORD = 'UserNamePassword';
const TEXT_PROMPT = 'textPrompt';

/**
 * UserNamePasswordDialog class handles the username and password authentication process.
 */
class UserNamePasswordDialog extends ComponentDialog {
    /**
     * Creates a new instance of the UserNamePasswordDialog class.
     * @param {string} id - The dialog ID.
     */
    constructor(id) {
        super(id);
        this.baseUrl = process.env.ApplicationBaseUrl;
        this.addDialog(new TextPrompt(TEXT_PROMPT));
        this.addDialog(new WaterfallDialog(USERNAMEPASSWORD, [
            this.promptStep.bind(this),
            this.loginStep.bind(this)
        ]));
        this.initialDialogId = USERNAMEPASSWORD;
    }

    /**
     * The promptStep method handles the initial prompt step of the dialog.
     * @param {Object} stepContext - The context of the current step.
     */
    async promptStep(stepContext) {
        if (stepContext.context._activity.value !== undefined) {
            const userDetails = JSON.parse(stepContext.context._activity.value.state);
            if (userDetails.userName === "testaccount@test123.onmicrosoft.com" && userDetails.password === "testpassword") {
                const messageText = 'What is your user name?';
                const msg = MessageFactory.text(messageText, messageText, InputHints.ExpectingInput);
                await stepContext.context.sendActivity('Login successful.');
                return await stepContext.prompt(TEXT_PROMPT, { prompt: msg });
            } else {
                await stepContext.context.sendActivity("Invalid credentials");
                return await stepContext.endDialog();
            }
        }

        await stepContext.context.sendActivity({ attachments: [this.getAdaptiveCardUserLogin()] });
        return await stepContext.endDialog();
    }

    /**
     * The loginStep method handles the login step after the prompt.
     * @param {Object} stepContext - The context of the current step.
     */
    async loginStep(stepContext) {
        const userName = stepContext.result;
        const userCard = CardFactory.adaptiveCard(this.getAdaptiveCardUserDetails(userName));
        await stepContext.context.sendActivity({ attachments: [userCard] });
        return await stepContext.endDialog();
    }

    /**
     * Generates an adaptive card with user details.
     * @param {string} userName - The user's name.
     * @returns {Object} - The adaptive card JSON.
     */
    getAdaptiveCardUserDetails(userName) {
        return {
            $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
            body: [
                {
                    type: "TextBlock",
                    size: "Medium",
                    weight: "Bolder",
                    text: "User profile details are"
                },
                {
                    type: "Image",
                    size: "Medium",
                    url: "https://media.istockphoto.com/vectors/profile-placeholder-image-gray-silhouette-no-photo-vector-id1016744034?k=20&m=1016744034&s=612x612&w=0&h=kjCAwH5GOC3n3YRTHBaLDsLIuF8P3kkAJc9RvfiYWBY="
                },
                {
                    type: "TextBlock",
                    size: "Medium",
                    weight: "Bolder",
                    wrap: true,
                    text: "Hello! Test user"
                },
                {
                    type: "TextBlock",
                    size: "Medium",
                    weight: "Bolder",
                    text: "Job title: Data scientist"
                },
                {
                    type: "TextBlock",
                    size: "Medium",
                    weight: "Bolder",
                    text: 'Email: testaccount@test123.onmicrosoft.com'
                },
                {
                    type: "TextBlock",
                    size: "Medium",
                    weight: "Bolder",
                    text: 'User name: ' + userName
                }
            ],
            type: 'AdaptiveCard',
            version: '1.4'
        };
    }

    /**
     * Generates an adaptive card for user login.
     * @returns {Object} - The adaptive card JSON.
     */
    getAdaptiveCardUserLogin() {
        return CardFactory.heroCard(
            'Signin card',
            undefined,
            CardFactory.actions([
                {
                    type: 'signin',
                    title: 'Get started',
                    value: `${this.baseUrl}/popUpSignin?from=bot&height=535&width=600`
                }
            ])
        );
    }
}

exports.UserNamePasswordDialog = UserNamePasswordDialog;