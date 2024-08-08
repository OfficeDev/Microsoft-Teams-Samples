//- Copyright (c) Microsoft Corporation.
//- All rights reserved.
// microsoftTeams app Initilize
 microsoftTeams.app.initialize().then(() => {
    var taskModuleButtons = document.getElementsByClassName("taskModuleButton");
    if (taskModuleButtons.length > 0) {
        fetch(`${window.location.origin}/getAppConfig`).then(response => response.json()).then(data => {

        //config your appid and baseurl
        var config = {
            MicrosoftAppID: data.MicrosoftAppId,
            BaseUrl: `${window.location.origin}`
        }

        var taskInfo_1 = {
            title: "",
            size: "",
            url: "",
            card: "",
            fallbackUrl: "",
            completionBotId: config.MicrosoftAppID
        };

        var TaskModuleIds = {
            YouTube: "youtube",
            PowerApp: "powerapp",
            CustomForm: "customform",
            AdaptiveCard1: "adaptivecard1",
            AdaptiveCard2: "adaptivecard2"
        };

        var DeelplinkIds = {
            CustomForm: "customform.html",
        };

        //Titles for taskmodule
        var TaskModuleStrings = {
            YouTubeTitle: "Microsoft Ignite 2018 Vision Keynote",
            PowerAppTitle: "PowerApp: Asset Checkout",
            CustomFormTitle: "Custom Form",
            AdaptiveCardTitle: "Create a new job posting",
            AdaptiveCardKitchenSinkTitle: "Adaptive Card: Inputs",
            ActionSubmitResponseTitle: "Action.Submit Response",
            YouTubeName: "YouTube",
            PowerAppName: "PowerApp",
            CustomFormName: "Custom Form",
            AdaptiveCardSingleName: "Adaptive Card - Single",
            AdaptiveCardSequenceName: "Adaptive Card - Sequence"
        };

        //Sizes for taskmodule
        var TaskModuleSizes = {
            youtube: {
                width: 1000,
                height: 700
            },
            powerapp: {
                width: 720,
                height: 520
            },
            customform: {
                width: 510,
                height: 430
            },
            adaptivecard: {
                width: 700,
                height: 255
            }
        };

        function appRoot() {
            if (typeof window === "undefined") {
                return config.BaseUrl;
            }
            else {
                return window.location.protocol + "//" + window.location.host;
            }
        }

        // Initilize Deeplink
        var deepLink = document.getElementById("deeplink");
        deepLink.href = "https://teams.microsoft.com/l/task/" + config.MicrosoftAppID + "?url=" + "".concat((0, appRoot)(), "/").concat(DeelplinkIds.CustomForm + "&height=" + TaskModuleSizes.customform.height + "&width=" + TaskModuleSizes.customform.width + "&title=" + TaskModuleStrings.CustomFormTitle + "&completionBotId=" + config.MicrosoftAppID);

        //Adaptive Card Template
        var adaptivecardTemplate = {
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "TextBlock",
                    "separator": true,
                    "size": "Large",
                    "weight": "Bolder",
                    "text": "Enter basic information for this position:",
                    "isSubtle": true,
                    "wrap": true
                },
                {
                    "type": "TextBlock",
                    "separator": true,
                    "text": "Title",
                    "wrap": true
                },
                {
                    "type": "Input.Text",
                    "id": "jobTitle",
                    "placeholder": "E.g. Senior PM"
                },
                {
                    "type": "ColumnSet",
                    "columns": [
                        {
                            "type": "Column",
                            "items": [
                                {
                                    "type": "TextBlock",
                                    "text": "Level",
                                    "wrap": true
                                },
                                {
                                    "type": "Input.Number",
                                    "id": "jobLevel",
                                    "value": "7",
                                    "placeholder": "Level in numbers min **1** and max **10**",
                                    "min": 1,
                                    "max": 10
                                }
                            ],
                            "width": 2
                        },
                        {
                            "type": "Column",
                            "items": [
                                {
                                    "type": "TextBlock",
                                    "text": "Location"
                                },
                                {
                                    "type": "Input.ChoiceSet",
                                    "id": "jobLocation",
                                    "value": "1",
                                    "choices": [
                                        {
                                            "title": "San Francisco",
                                            "value": "1"
                                        },
                                        {
                                            "title": "London",
                                            "value": "2"
                                        },
                                        {
                                            "title": "Singapore",
                                            "value": "3"
                                        },
                                        {
                                            "title": "Dubai",
                                            "value": "3"
                                        },
                                        {
                                            "title": "Frankfurt",
                                            "value": "3"
                                        }
                                    ],
                                    "isCompact": true
                                }
                            ],
                            "width": 2
                        }
                    ]
                }
            ],
            "actions": [
                {
                    "type": "Action.Submit",
                    "id": "createPosting",
                    "title": "Create posting",
                    "data": {
                        "command": "createPosting",
                        "taskResponse": "{{responseType}}"
                    }
                },
                {
                    "type": "Action.Submit",
                    "id": "cancel",
                    "title": "Cancel"
                }
            ],
            "version": "1.0"
        }

        //buttons addEventListener
        for (var _i = 0, taskModuleButtons_1 = taskModuleButtons; _i < taskModuleButtons_1.length; _i++) {
            var btn = taskModuleButtons_1[_i];
            btn.addEventListener("click", function () {
                taskInfo_1.url = "".concat((0, appRoot)(), "/").concat(this.id.toLowerCase(), ".html");

                // Define default submitHandler()
                var submitHandler = function (err, result) { console.log("Err: ".concat(err, "; Result:  + ").concat(result)); };
                switch (this.id.toLowerCase()) {

                    case TaskModuleIds.YouTube:
                        taskInfo_1.title = TaskModuleStrings.YouTubeTitle;
                        taskInfo_1.size = { height: 7000, width: 1000 };
                        microsoftTeams.dialog.open(taskInfo_1, submitHandler);
                        break;

                    case TaskModuleIds.PowerApp:
                        taskInfo_1.title = TaskModuleStrings.PowerAppTitle;
                        taskInfo_1.size = { height: TaskModuleSizes.powerapp.height, width: TaskModuleSizes.powerapp.width };
                        microsoftTeams.dialog.open(taskInfo_1, submitHandler);
                        break;

                    case TaskModuleIds.CustomForm:
                        taskInfo_1.title = TaskModuleStrings.CustomFormTitle;
                        taskInfo_1.size = { height: TaskModuleSizes.customform.height, width: TaskModuleSizes.customform.width };

                        //submitHandler callback function
                        submitHandler = (err, result) => {
                            console.log(`Submit handler - err: ${err}`);
                            console.log(`Submit handler - result\rName: ${result.name}\rEmail: ${result.email}\rFavorite book: ${result.favoriteBook}`);
                        };

                        //Allows app to open a url based dialog.
                        microsoftTeams.tasks.startTask(taskInfo_1, submitHandler);
                        break;

                    case TaskModuleIds.AdaptiveCard1:
                        taskInfo_1.title = TaskModuleStrings.AdaptiveCardTitle;
                        taskInfo_1.url = "";
                        taskInfo_1.size = { height: TaskModuleSizes.powerapp.height, width: TaskModuleSizes.powerapp.width };
                        taskInfo_1.card = adaptivecardTemplate;

                        //submitHandler callback function
                        submitHandler = (err, result) => {
                            console.log(`Submit handler - err: ${err}`);
                            console.log(
                                "Result = " + JSON.stringify(result) + "\nError = " + JSON.stringify(err)
                            );
                        };

                        //Dialogs (referred as dialogs (referred as task modules in TeamsJS v1.x) in TeamsJS v1.x) invoked from a tab
                        microsoftTeams.dialog.open(taskInfo_1, submitHandler);
                        break;

                    default:
                        console.log("Unexpected button ID: " + this.id.toLowerCase());
                        return;
                }
                console.log("URL: " + taskInfo_1.url);
            });
        }
    });
    }
});