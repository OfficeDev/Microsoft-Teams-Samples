// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

import * as builder from "botbuilder";
import * as constants from "../constants";
import * as utils from "../utils";
import * as logger from "winston";
import { ACGeneratorDialog } from "./ACGenerator";
import { renderACAttachment } from "../utils/CardUtils";
import { cardTemplates, fetchTemplates, appRoot } from "./CardTemplates";
import { taskModuleLink } from "../utils/DeepLinks";
import { BotFrameworkCard } from "./BotFrameworkCard";
// The Adaptive Card version is shown by default, which is why this lives in RootDialog
export class RootDialog extends builder.IntentDialog
{
    constructor() {
        super();
    }

    // Register the dialogs with the bot
    public register(bot: builder.UniversalBot): void {
        bot.dialog(constants.DialogId.Root, this);

        this.onBegin((session, args, next) => { logger.verbose("onDialogBegin called"); this.onDialogBegin(session, args, next); });
        this.onDefault((session) => { logger.verbose("onDefault called"); this.onMessageReceived(session); } );
        new ACGeneratorDialog(constants.DialogId.ACTester).register(bot, this);
        this.matches(/actester/i, constants.DialogId.ACTester);
        new BotFrameworkCard(constants.DialogId.BFCard).register(bot, this);
        this.matches(/bfcard/i, constants.DialogId.BFCard);
    }

    // Handle start of dialog
    private async onDialogBegin(session: builder.Session, args: any, next: () => void): Promise<void> {
        next();
    }

    // Handle message
    private async onMessageReceived(session: builder.Session): Promise<void> {
        console.log("Context: " + JSON.stringify(utils.getContext(null, session)));
        if (session.message.text === "") {
            console.log("Empty message received");
            // This is a response from a generated AC card
            if (session.message.value !== undefined) {
                session.send("**Action.Submit results:**\n```" + JSON.stringify(session.message.value) + "```");
            }
        }
        else {
            // If the user says anything, return a card to kick off Task Module flows
            // Message might contain @mentions which we would like to strip off in the response
            let text = utils.getTextWithoutMentions(session.message);

            let appInfo = {
                appId: process.env.MICROSOFT_APP_ID as string,
            };
            let taskModuleUrls = {
                url1: taskModuleLink(appInfo.appId, constants.TaskModuleStrings.YouTubeTitle, constants.TaskModuleSizes.youtube.height, constants.TaskModuleSizes.youtube.width, `${appRoot()}/${constants.TaskModuleIds.YouTube}`, null, `${appRoot()}/${constants.TaskModuleIds.YouTube}`),
                url2: taskModuleLink(appInfo.appId, constants.TaskModuleStrings.PowerAppTitle, constants.TaskModuleSizes.powerapp.height, constants.TaskModuleSizes.powerapp.width, `${appRoot()}/${constants.TaskModuleIds.PowerApp}`, null, `${appRoot()}/${constants.TaskModuleIds.PowerApp}`),
                url3: taskModuleLink(appInfo.appId, constants.TaskModuleStrings.CustomFormTitle, constants.TaskModuleSizes.customform.height, constants.TaskModuleSizes.customform.width, `${appRoot()}/${constants.TaskModuleIds.CustomForm}`, null, `${appRoot()}/${constants.TaskModuleIds.CustomForm}`, appInfo.appId),
                url4: taskModuleLink(appInfo.appId, constants.TaskModuleStrings.AdaptiveCardTitle, constants.TaskModuleSizes.adaptivecard.height, constants.TaskModuleSizes.adaptivecard.width, null, cardTemplates.adaptiveCard, null, appInfo.appId),
                url5: taskModuleLink(appInfo.appId, constants.TaskModuleStrings.AdaptiveCardTitle, constants.TaskModuleSizes.adaptivecard.height, constants.TaskModuleSizes.adaptivecard.width, null, cardTemplates.adaptiveCard, null, appInfo.appId),
            };

            let cardData: any = {
                title: "Task Module - Adaptive Card",
                subTitle: "Task Module Test Card",
                instructions: "Click on the buttons below below to open task modules in various ways. Type 'bfcard' to see the Bot Framework card version.",
                linkbutton1: constants.TaskModuleStrings.YouTubeName,
                url1: taskModuleUrls.url1,
                markdown1: `[${constants.TaskModuleStrings.YouTubeName}](${taskModuleUrls.url1})`,
                linkbutton2: constants.TaskModuleStrings.PowerAppName,
                url2: taskModuleUrls.url2,
                markdown2: `[${constants.TaskModuleStrings.PowerAppName}](${taskModuleUrls.url2})`,
                linkbutton3: constants.TaskModuleStrings.CustomFormName,
                url3: taskModuleUrls.url3,
                markdown3: `[${constants.TaskModuleStrings.CustomFormName}](${taskModuleUrls.url3})`,
                linkbutton4: constants.TaskModuleStrings.AdaptiveCardSingleName,
                url4: taskModuleUrls.url4,
                markdown4: `[${constants.TaskModuleStrings.AdaptiveCardSingleName}](${taskModuleUrls.url4})`,
                linkbutton5: constants.TaskModuleStrings.AdaptiveCardSequenceName,
                url5: taskModuleUrls.url5,
                markdown5: `[${constants.TaskModuleStrings.AdaptiveCardSequenceName}](${taskModuleUrls.url5})`,
                fetchButtonId1: `${constants.TaskModuleIds.YouTube}`,
                fetchButtonId2: `${constants.TaskModuleIds.PowerApp}`,
                fetchButtonId3: `${constants.TaskModuleIds.CustomForm}`,
                fetchButtonId4: `${constants.TaskModuleIds.AdaptiveCard1}`,
                fetchButtonId5: `${constants.TaskModuleIds.AdaptiveCard2}`,
                fetchButtonTitle1: `${constants.TaskModuleStrings.YouTubeName}`,
                fetchButtonTitle2: `${constants.TaskModuleStrings.PowerAppName}`,
                fetchButtonTitle3: `${constants.TaskModuleStrings.CustomFormName}`,
                fetchButtonTitle4: `${constants.TaskModuleStrings.AdaptiveCardSingleName}`,
                fetchButtonTitle5: `${constants.TaskModuleStrings.AdaptiveCardSequenceName}`,
                tfJsonTitle1: `${constants.TaskModuleStrings.YouTubeName}`,
                tfJson1: `${JSON.stringify(fetchTemplates[constants.TaskModuleIds.YouTube])}`,
                tfJsonTitle2: `${constants.TaskModuleStrings.PowerAppName}`,
                tfJson2: `${JSON.stringify(fetchTemplates[constants.TaskModuleIds.PowerApp])}`,
                tfJsonTitle3: `${constants.TaskModuleStrings.CustomFormName}`,
                tfJson3: `${JSON.stringify(fetchTemplates[constants.TaskModuleIds.CustomForm])}`,
            };

            session.send(new builder.Message(session).addAttachment(
                renderACAttachment(cardTemplates.taskModule, cardData),
            ));
            // session.send("You said: %s", text);
        }
    }
}
