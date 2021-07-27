const { StatusCodes } = require('botbuilder');
var fs = require("fs");

// Card response for authentication
const createAuthResponse = (signInLink) => {
    console.log("Create Auth response")
    const res = {
        status: StatusCodes.OK,
        body: {
            "tab": {
                "type": "auth",
                "suggestedActions": {
                    "actions": [
                        {
                            "type": "openUrl",
                            "value": signInLink,
                            "title": "Sign in to this app"
                        }
                    ]
                }
            },
        }
    };
    return res;
};

// Card response for task module invoke request
const invokeTaskResponse = () => {
    const response = {
        status: StatusCodes.OK,
        body: {
            task: {
                type: 'continue',
                value: {
                    card: {
                        contentType: "application/vnd.microsoft.card.adaptive",
                        content: adaptiveCardTaskModule
                    },
                    heigth: 250,
                    width: 400,
                    title: 'Sample Adaptive Card'
                }
            }
        }
    };
    return response;
};

// Card response for tab fetch request
const createFetchResponse = async (userImage, displayName) => {
    console.log("Create Invoke response");
    var imageString = '';

    if (userImage) {
        // Converting image of Blob type to base64 string for rendering as image.
        await userImage.arrayBuffer().then(result => {
            console.log(userImage.type);
            imageString = Buffer.from(result).toString('base64');
            if (imageString != '') {
                // Writing file to Images folder to use as url in adaptive card
                fs.writeFileSync("Images/profile-image.jpeg", imageString, { encoding: 'base64' }, function (err) {
                    console.log("File Created");
                });
            }
        }).catch(error => { console.log(error) });
    }

    const res = {
        status: StatusCodes.OK,
        body: {
            "tab": {
                "type": "continue",
                "value": {
                    "cards": [
                        {
                            "card": getAdaptiveCardUserDetails(imageString, displayName),
                        },
                        {
                            "card": getAdaptiveCardSubmitAction(),
                        }
                    ]
                },
            },
        }
    };

    return res;
};

// Card response for tab submit request
const createSubmitResponse = () => {
    console.log("Submit response")
    const res = {
        status: StatusCodes.OK,
        body: {
            "tab": {
                "type": "continue",
                "value": {
                    "cards": [
                        {
                            "card": signOutCard,
                        }
                    ]
                },
            },
        }
    };

    return res;
};

// Card response for tab submit request
const taskSubmitResponse = () => {
    console.log("Task Submit response")
    const res = {
        status: StatusCodes.OK,
        body: {
            "task": {
                "value": {
                    "tab": {
                        "type": "continue",
                        "value": {
                            "cards": [
                                {
                                    "card": taskSubmitCard
                                }
                            ]
                        }
                    }
                },
                "type": "continue"
            },
            "responseType": "task"
        }
    };

    return res;
};

// Adaptive Card with user image, name and Task Module invoke action
const getAdaptiveCardUserDetails = (image, name) => {
    const adaptiveCard1 = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        body: [
            {
                type: "ColumnSet",
                columns: [
                    {
                        type: "Column",
                        items: [
                            {
                                "type": "Image",
                                "url": image && image != '' ? process.env.BaseUrl + "/Images/profile-image.jpeg" : "https://cdn.vox-cdn.com/thumbor/Ndb49Uk3hjiquS041NDD0tPDPAs=/0x169:1423x914/fit-in/1200x630/cdn.vox-cdn.com/uploads/chorus_asset/file/7342855/microsoftteams.0.jpg",
                                "size": "Medium"
                            }
                        ],
                        width: "auto"
                    },
                    {
                        type: "Column",
                        items: [
                            {
                                "type": "TextBlock",
                                "weight": "Bolder",
                                "text": 'Hello: ' + name,
                                "wrap": true
                            },
                        ],
                        "width": "stretch"
                    }
                ]
            },
            {
                type: 'ActionSet',
                actions: [
                    {
                        type: "Action.Submit",
                        title: "Show Task Module",
                        data: {
                            msteams: {
                                type: "task/fetch"
                            }
                        }
                    }
                ]
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return adaptiveCard1;
}

// Adaptive Card showing sample text and Submit Action
const getAdaptiveCardSubmitAction = () => {
    const adaptiveCard2 = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        body: [
            {
                type: 'Image',
                height: '300px',
                width: '400px',
                url: 'https://cdn.vox-cdn.com/thumbor/Ndb49Uk3hjiquS041NDD0tPDPAs=/0x169:1423x914/fit-in/1200x630/cdn.vox-cdn.com/uploads/chorus_asset/file/7342855/microsoftteams.0.jpg',
            },
            {
                type: 'TextBlock',
                size: 'Medium',
                weight: 'Bolder',
                text: 'tab/fetch is the first invoke request that your bot receives when a user opens an Adaptive Card tab. When your bot receives the request, it either sends a tab continue response or a tab auth response',
                wrap: true,
            },
            {
                type: 'TextBlock',
                size: 'Medium',
                weight: 'Bolder',
                text: 'tab/submit request is triggered to your bot with the corresponding data through the Action.Submit function of Adaptive Card',
                wrap: true,
            },
            {
                type: 'ActionSet',
                actions: [
                    {
                        type: 'Action.Submit',
                        title: 'Sign Out',
                    }
                ],
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return adaptiveCard2;
}

// Adaptive Card to show in task module
const adaptiveCardTaskModule = {
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: 'Sample task module flow for tab'
        },
        {
            type: 'Image',
            height: '50px',
            width: '50px',
            url: 'https://cdn.vox-cdn.com/thumbor/Ndb49Uk3hjiquS041NDD0tPDPAs=/0x169:1423x914/fit-in/1200x630/cdn.vox-cdn.com/uploads/chorus_asset/file/7342855/microsoftteams.0.jpg',
        },
        {
            type: 'ActionSet',
            actions: [
                {
                    type: "Action.Submit",
                    title: "Close",
                    data: {
                        msteams: {
                            type: "task/submit"
                        }
                    }
                }
            ]
        }
    ],
    type: 'AdaptiveCard',
    version: '1.4'
};

// Adaptive Card to show sign out action
const signOutCard = {
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: 'Sign out successful. Please refresh to Sign in again.',
            wrap: true,
        }
    ],
    type: 'AdaptiveCard',
    version: '1.4'
};

// Adaptive Card to show task/submit action
const taskSubmitCard = {
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: 'The action called task/submit. Please refresh to laod contents again.',
            wrap: true,
        }
    ],
    type: 'AdaptiveCard',
    version: '1.4'
};

module.exports = {
    createFetchResponse,
    createSubmitResponse,
    createAuthResponse,
    invokeTaskResponse,
    taskSubmitResponse
};