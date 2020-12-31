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
import { cardTemplates, fetchTemplates, appRoot } from "./CardTemplates";
import { taskModuleLink } from "../utils/DeepLinks";
import { renderCard } from "../utils/CardUtils";

// Dialog for the Adaptive Card tester
export class BotFrameworkCard extends builder.IntentDialog
{
    constructor(private dialogId: string) {
        super({ recognizeMode: builder.RecognizeMode.onBegin });
    }

    public register(bot: builder.UniversalBot, rootDialog: builder.IntentDialog): void {
        bot.dialog(this.dialogId, this);

        this.onBegin((session, args, next) => { this.onDialogBegin(session, args, next); });
        this.onDefault((session) => { this.onMessageReceived(session); });
    }

    // Handle start of dialog
    private async onDialogBegin(session: builder.Session, args: any, next: () => void): Promise<void> {
        next();
    }

    // Handle message
    private async onMessageReceived(session: builder.Session): Promise<void> {
        // Message might contain @mentions which we would like to strip off in the response
        let text = utils.getTextWithoutMentions(session.message);

        let appInfo = {
            appId: process.env.MICROSOFT_APP_ID,
        };
        let taskModuleUrls = {
            url1: taskModuleLink(appInfo.appId, constants.TaskModuleStrings.YouTubeTitle, constants.TaskModuleSizes.youtube.height, constants.TaskModuleSizes.youtube.width, `${appRoot()}/${constants.TaskModuleIds.YouTube}`),
            url2: taskModuleLink(appInfo.appId, constants.TaskModuleStrings.PowerAppTitle, constants.TaskModuleSizes.powerapp.height, constants.TaskModuleSizes.powerapp.width, `${appRoot()}/${constants.TaskModuleIds.PowerApp}`),
            url3: taskModuleLink(appInfo.appId, constants.TaskModuleStrings.CustomFormTitle, constants.TaskModuleSizes.customform.height, constants.TaskModuleSizes.customform.width, `${appRoot()}/${constants.TaskModuleIds.CustomForm}`),
            url4: taskModuleLink(appInfo.appId, constants.TaskModuleStrings.AdaptiveCardTitle, constants.TaskModuleSizes.adaptivecard.height, constants.TaskModuleSizes.adaptivecard.width, null, cardTemplates.adaptiveCard),
            url5: taskModuleLink(appInfo.appId, constants.TaskModuleStrings.AdaptiveCardTitle, constants.TaskModuleSizes.adaptivecard.height, constants.TaskModuleSizes.adaptivecard.width, null, cardTemplates.adaptiveCard),
        };

        let cardData: any = {
            title: "Task Module - Bot Framework",
            subTitleDL: "Deep Links",
            instructionsDL: "Click on the buttons below below to open a task module via deep link.",
            subTitleTF: "Invoke: task/fetch",
            instructionsTF: "Click on the buttons below below to open a task module via task/fetch.",
            linkbutton1: constants.TaskModuleStrings.YouTubeName,
            url1: taskModuleUrls.url1,
            linkbutton2: constants.TaskModuleStrings.PowerAppName,
            url2: taskModuleUrls.url2,
            linkbutton3: constants.TaskModuleStrings.CustomFormName,
            url3: taskModuleUrls.url3,
            linkbutton4: constants.TaskModuleStrings.AdaptiveCardSingleName,
            url4: taskModuleUrls.url4,
            linkbutton5: constants.TaskModuleStrings.AdaptiveCardSequenceName,
            url5: taskModuleUrls.url5,
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
        };

        if (text === constants.DialogId.BFCard) {
            // The user has typed "bfcard" - send two cards, one illustrating deep link buttons, and one with task/fetch
            session.send(new builder.Message(session).addAttachment(
                renderCard(cardTemplates.bfThumbnailDeepLink, cardData),
            ));
            session.send(new builder.Message(session).addAttachment(
                renderCard(cardTemplates.bfThumbnailTaskFetch, cardData),
            ));
        }
        session.endDialog();
    }
}
