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
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//

import * as microsoftTeams from "@microsoft/teams-js";
import * as constants from "./constants";
import { cardTemplates, appRoot } from "./dialogs/CardTemplates";
import { taskModuleLink } from "./utils/DeepLinks";

declare var appId: any; // Injected at template render time

// Helper function for generating an adaptive card attachment
function acAttachment(ac: any): any {
    return {
        contentType: "application/vnd.microsoft.card.adaptive",
        content: ac,
    };
}

// Set the desired theme
function setTheme(theme: string): void {
    if (theme) {
        // Possible values for theme: 'default', 'light', 'dark' and 'contrast'
        document.body.className = "theme-" + (theme === "default" ? "light" : theme);
    }
}

// Create the URL that Microsoft Teams will load in the tab. You can compose any URL even with query strings.
function createTabUrl(): string {
    let tabChoice = document.getElementById("tabChoice");
    let selectedTab = tabChoice[(tabChoice as HTMLSelectElement).selectedIndex].value;

    return window.location.protocol + "//" + window.location.host + "/" + selectedTab;
}

// Call the initialize API first
microsoftTeams.initialize();

// Check the initial theme user chose and respect it
microsoftTeams.getContext(function(context: microsoftTeams.Context): void {
    if (context && context.theme) {
        setTheme(context.theme);
    }
});

// Handle theme changes
microsoftTeams.registerOnThemeChangeHandler(function(theme: string): void {
    setTheme(theme);
});

// Save configuration changes
microsoftTeams.settings.registerOnSaveHandler(function(saveEvent: microsoftTeams.settings.SaveEvent): void {
    // Let the Microsoft Teams platform know what you want to load based on
    // what the user configured on this page
    microsoftTeams.settings.setSettings({
        contentUrl: createTabUrl(), // Mandatory parameter
        entityId: createTabUrl(), // Mandatory parameter
    });

    // Tells Microsoft Teams platform that we are done saving our settings. Microsoft Teams waits
    // for the app to call this API before it dismisses the dialog. If the wait times out, you will
    // see an error indicating that the configuration settings could not be saved.
    saveEvent.notifySuccess();
});

// Logic to let the user configure what they want to see in the tab being loaded
document.addEventListener("DOMContentLoaded", function(): void {
    // This module runs on multiple pages, so we need to isolate page-specific logic.

    // If we are on the tab configuration page, wire up the save button initialization state
    let tabChoice = document.getElementById("tabChoice");
    if (tabChoice) {
        tabChoice.onchange = function(): void {
            let selectedTab = this[(this as HTMLSelectElement).selectedIndex].value;

            // This API tells Microsoft Teams to enable the 'Save' button. Since Microsoft Teams always assumes
            // an initial invalid state, without this call the 'Save' button will never be enabled.
            microsoftTeams.settings.setValidityState(selectedTab === "first" || selectedTab === "second" || selectedTab === "taskmodule");
        };
    }

    // If we are on the Task Module page, initialize the buttons and deep links
    let taskModuleButtons = document.getElementsByClassName("taskModuleButton");
    if (taskModuleButtons.length > 0) {
        // Initialize deep links
        let taskInfo = {
            title: null,
            height: null,
            width: null,
            url: null,
            card: null,
            fallbackUrl: null,
            completionBotId: null,
        };
        let deepLink = document.getElementById("dlYouTube") as HTMLAnchorElement;
        deepLink.href = taskModuleLink(appId, constants.TaskModuleStrings.YouTubeTitle, constants.TaskModuleSizes.youtube.height, constants.TaskModuleSizes.youtube.width, `${appRoot()}/${constants.TaskModuleIds.YouTube}`, null, `${appRoot()}/${constants.TaskModuleIds.YouTube}`);
        deepLink = document.getElementById("dlPowerApps") as HTMLAnchorElement;
        deepLink.href = taskModuleLink(appId, constants.TaskModuleStrings.PowerAppTitle, constants.TaskModuleSizes.powerapp.height, constants.TaskModuleSizes.powerapp.width, `${appRoot()}/${constants.TaskModuleIds.PowerApp}`, null, `${appRoot()}/${constants.TaskModuleIds.PowerApp}`);
        deepLink = document.getElementById("dlCustomForm") as HTMLAnchorElement;
        deepLink.href = taskModuleLink(appId, constants.TaskModuleStrings.CustomFormTitle, constants.TaskModuleSizes.customform.height, constants.TaskModuleSizes.customform.width, `${appRoot()}/${constants.TaskModuleIds.CustomForm}`, null, `${appRoot()}/${constants.TaskModuleIds.CustomForm}`);
        deepLink = document.getElementById("dlAdaptiveCard1") as HTMLAnchorElement;
        deepLink.href = taskModuleLink(appId, constants.TaskModuleStrings.AdaptiveCardTitle, constants.TaskModuleSizes.adaptivecard.height, constants.TaskModuleSizes.adaptivecard.width, null, cardTemplates.adaptiveCard);
        deepLink = document.getElementById("dlAdaptiveCard2") as HTMLAnchorElement;
        deepLink.href = taskModuleLink(appId, constants.TaskModuleStrings.AdaptiveCardTitle, constants.TaskModuleSizes.adaptivecard.height, constants.TaskModuleSizes.adaptivecard.width, null, cardTemplates.adaptiveCard, null, appId);

        for (let btn of taskModuleButtons) {
            btn.addEventListener("click",
                function (): void {
                    // Hide customFormResults, adaptiveResults
                    document.getElementById("customFormResults").style.display = "none";
                    document.getElementById("adaptiveResults").style.display = "none";
                    taskInfo.url = `${appRoot()}/${this.id.toLowerCase()}?theme={theme}`;
                    // Define default submitHandler()
                    let submitHandler = (err: string, result: any): void => { console.log(`Err: ${err}; Result:  + ${result}`); };
                    switch (this.id.toLowerCase()) {
                        case constants.TaskModuleIds.YouTube:
                            taskInfo.title = constants.TaskModuleStrings.YouTubeTitle;
                            taskInfo.height = constants.TaskModuleSizes.youtube.height;
                            taskInfo.width = constants.TaskModuleSizes.youtube.width;
                            microsoftTeams.tasks.startTask(taskInfo, submitHandler);
                            break;
                        case constants.TaskModuleIds.PowerApp:
                            taskInfo.title = constants.TaskModuleStrings.PowerAppTitle;
                            taskInfo.height = constants.TaskModuleSizes.powerapp.height;
                            taskInfo.width = constants.TaskModuleSizes.powerapp.width;
                            microsoftTeams.tasks.startTask(taskInfo, submitHandler);
                            break;
                        case constants.TaskModuleIds.CustomForm:
                            taskInfo.title = constants.TaskModuleStrings.CustomFormTitle;
                            taskInfo.height = constants.TaskModuleSizes.customform.height;
                            taskInfo.width = constants.TaskModuleSizes.customform.width;
                            submitHandler = (err: string, result: any): void => {
                                // Unhide and populate customFormResults
                                let resultsElement = document.getElementById("customFormResults");
                                resultsElement.style.display = "block";
                                if (err) {
                                    resultsElement.innerHTML = `Error/Cancel: ${err}`;
                                }
                                if (result) {
                                    resultsElement.innerHTML = `Result: Name: "${result.name}"; Email: "${result.email}"; Favorite book: "${result.favoriteBook}"`;
                                }
                            };
                            microsoftTeams.tasks.startTask(taskInfo, submitHandler);
                            break;
                        case constants.TaskModuleIds.AdaptiveCard1:
                            taskInfo.title = constants.TaskModuleStrings.AdaptiveCardTitle;
                            taskInfo.url = null;
                            taskInfo.height = constants.TaskModuleSizes.adaptivecard.height;
                            taskInfo.width = constants.TaskModuleSizes.adaptivecard.width;
                            taskInfo.card = acAttachment(cardTemplates.adaptiveCard);
                            submitHandler = (err: string, result: any): void => {
                                // Unhide and populate adaptiveResults
                                let resultsElement = document.getElementById("adaptiveResults");
                                resultsElement.style.display = "block";
                                if (err) {
                                    resultsElement.innerHTML = `Error/Cancel: ${err}`;
                                }
                                if (result) {
                                    resultsElement.innerHTML = `Result: ${JSON.stringify(result)}`;
                                }
                            };
                            microsoftTeams.tasks.startTask(taskInfo, submitHandler);
                            break;
                        case constants.TaskModuleIds.AdaptiveCard2:
                            taskInfo.title = constants.TaskModuleStrings.AdaptiveCardTitle;
                            taskInfo.url = null;
                            taskInfo.height = constants.TaskModuleSizes.adaptivecard.height;
                            taskInfo.width = constants.TaskModuleSizes.adaptivecard.width;
                            taskInfo.card = acAttachment(cardTemplates.adaptiveCard);
                            // Send the Adaptive Card as filled in by the user to the bot in this app
                            taskInfo.completionBotId = appId;
                            microsoftTeams.tasks.startTask(taskInfo);
                            break;
                        default:
                            console.log("Unexpected button ID: " + this.id.toLowerCase());
                            return;
                    }
                    console.log("URL: " + taskInfo.url);
                });
        }
    }
});
