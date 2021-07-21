const { StatusCodes } = require('botbuilder');

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
    var profileImageUrl = '';

    // Converting image of Blob type to base64 string for rendering as image.
    await userImage.arrayBuffer().then(result => {
        console.log(userImage.type);
        const imageBytes = Buffer.from(result).toString('base64');
        profileImageUrl = `data:${userImage.type};base64,${imageBytes}`;
    }).catch(error => { console.log(error) });

    const res = {
        status: StatusCodes.OK,
        body: {
            "tab": {
                "type": "continue",
                "value": {
                    "cards": [
                        {
                            "card": getAdaptiveCard1(profileImageUrl, displayName),
                        },
                        {
                            "card": getAdaptiveCard2(),
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

const getAdaptiveCard1 = (image, name) => {
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
                                "url": image ? `${image}` : "https://cdn.vox-cdn.com/thumbor/Ndb49Uk3hjiquS041NDD0tPDPAs=/0x169:1423x914/fit-in/1200x630/cdn.vox-cdn.com/uploads/chorus_asset/file/7342855/microsoftteams.0.jpg",
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

const getAdaptiveCard2 = () => {
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
        }
    ],
    type: 'AdaptiveCard',
    version: '1.4'
};

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

module.exports = {
    createFetchResponse,
    createSubmitResponse,
    createAuthResponse,
    invokeTaskResponse
};