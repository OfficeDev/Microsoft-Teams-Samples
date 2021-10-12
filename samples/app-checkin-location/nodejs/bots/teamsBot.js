// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const { TaskModuleResponseFactory } = require("../models/taskModuleResponseFactory");
const conversationDataReferences = {};

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member = 0; member < membersAdded.length; member++) {
                if (membersAdded[member].id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome! With this sample you can checkin your location (use command 'checkin') and view your checked in location(use command 'viewcheckin').");
                }
            }

            await next();
        });

        this.onMessage(async (context, next) => {
            if (context.activity.text.toLowerCase().trim() == "viewcheckin") {
                var currentData = conversationDataReferences["userDetails"];

                // Check if currentDatais empty.
                if (currentData == undefined) {
                    await context.sendActivity("No last check in found");
                }
                else {
                    // An array for cards will be created for all the checkin details of the user. 
                    const cardArray = new Array();
                    currentData.map((user) => {
                        let userCheckinCard = CardFactory.adaptiveCard(this.adaptiveCardForUserLastCheckin(user));
                        cardArray.push(userCheckinCard);
                    })

                    await context.sendActivity({ attachments: cardArray });
                }
            }
            else {
                const userCard = CardFactory.adaptiveCard(this.adaptiveCardForTaskModule());
                await context.sendActivity({ attachments: [userCard] });
            }

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    handleTeamsTaskModuleFetch(context, taskModuleRequest) {
        const cardTaskFetchId = taskModuleRequest.data.id;
        var taskInfo = {}; // TaskModuleTaskInfo

        if (cardTaskFetchId == "checkin") {
            taskInfo.url = taskInfo.fallbackUrl = this.baseUrl + "/CheckIn";
            taskInfo.height = 350;
            taskInfo.width = 350;
            taskInfo.title = "Check in details";
        }
        else if (cardTaskFetchId == "viewLocation") {
            let latitude = taskModuleRequest.data.latitude;
            let longitude = taskModuleRequest.data.longitude;
            taskInfo.url = taskInfo.fallbackUrl = this.baseUrl + "/ViewLocation?latitude=" + latitude + "&longitude=" + longitude;
            taskInfo.height = 350;
            taskInfo.width = 350;
            taskInfo.title = "View location";
        }

        return TaskModuleResponseFactory.toTaskModuleResponse(taskInfo);
    }

    async handleTeamsTaskModuleSubmit(context, taskModuleRequest) {
        // Create new object to save user's checkin details to file.
        let newCheckinDetails = {
            userId: context.activity.from.aadObjectId,
            userName: context.activity.from.name,
            latitude: taskModuleRequest.data.latitude,
            longitude: taskModuleRequest.data.longitude,
            time: context.activity.localTimestamp.toString()
        };

        await this.saveUserDetails(context, newCheckinDetails);
        const locationCard = CardFactory.adaptiveCard(this.adaptiveCardForUserLocationDetails(newCheckinDetails.userName, newCheckinDetails.time, newCheckinDetails.latitude, newCheckinDetails.longitude));
        await context.sendActivity({ attachments: [locationCard] });

        return null;
    }

    // This method is used to save user's checkin details.
    async saveUserDetails(context, newCheckinDetails) {
        var currentData = conversationDataReferences["userDetails"];
        // Check if currentData is undefined.
        if (currentData == undefined) {
            const userDetailsList = new Array();
            userDetailsList.push(newCheckinDetails)
            currentData = userDetailsList;
            conversationDataReferences["userDetails"] = currentData;
        }
        // Check if currentData length is 10.
        else if (currentData.length == 10) {
            currentData.splice(0, 1);
            const userDetailsList = currentData;
            userDetailsList.push(newCheckinDetails)
            currentData = userDetailsList;
            conversationDataReferences["userDetails"] = currentData;
        }
        else {
            const userDetailsList = currentData;
            userDetailsList.push(newCheckinDetails)
            currentData = userDetailsList;
            conversationDataReferences["userDetails"] = currentData;
        }
    }

    adaptiveCardForTaskModule = () => ({
        $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
        body: [
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: "Please click on check in"
            },
            {
                type: "ActionSet",
                actions: [
                    {
                        type: "Action.Submit",
                        title: "Check in",
                        data: {
                            msteams: {
                                type: "task/fetch"
                            },
                            id: "checkin"
                        }
                    }
                ]
            }
        ],
        type: "AdaptiveCard",
        version: "1.2"
    });

    adaptiveCardForUserLastCheckin = (userDetail) => ({
        $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
        body: [
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: `User name: ${userDetail.userName}`
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                wrap: "true",
                text: `Check in time: ${userDetail.time}`
            },
            {
                type: 'ActionSet',
                actions: [
                    {
                        type: "Action.Submit",
                        title: "View Location",
                        data: {
                            msteams: {
                                type: "task/fetch"
                            },
                            id: "viewLocation",
                            latitude: userDetail.latitude,
                            longitude: userDetail.longitude
                        }
                    }
                ]
            }
        ],
        type: "AdaptiveCard",
        version: "1.2"
    });

    adaptiveCardForUserLocationDetails = (userName, time, latitude, longitude) => ({
        $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
        body: [
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: `Username is: ${userName}`
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                wrap: "true",
                text: `Check in time: ${time}`
            },
            {
                type: "ActionSet",
                actions: [
                    {
                        type: "Action.Submit",
                        title: "View Location",
                        data: {
                            msteams: {
                                type: "task/fetch"
                            },
                            id: "viewLocation",
                            latitude: latitude,
                            longitude: longitude
                        }
                    }
                ]
            }
        ],
        type: "AdaptiveCard",
        version: "1.2"
    });
}

module.exports.TeamsBot = TeamsBot;