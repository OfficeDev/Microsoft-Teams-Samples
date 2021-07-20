const { StatusCodes } = require('botbuilder');

var profileImageUrl = '';
var userName = '';
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

const createFetchResponse = async (image, name) => {
    console.log("Create Invoke response")
    
    profileImageUrl = image;
    userName = name;
    const res = {
        status: StatusCodes.OK,
        body: {
            "tab": {
                "type": "continue",
                "value": {
                    "cards": [
                        {
                            "card": adaptiveCard1,
                        },
                        {
                            "card": adaptiveCard2,
                        }
                    ]
                },
            },
        }
    };

    return res;
};

const createSubmitResponse = () => {
    console.log("Create Invoke response")
    const res = {
        status: StatusCodes.OK,
        body: {
            "tab": {
                "type": "continue",
                "value": {
                    "cards": [
                        {
                            "card": adaptiveCard2,
                        }
                    ]
                },
            },
        }
    };

    return res;
};


const adaptiveCard1 = {
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: 'Hello: '
        },
        {
            type: 'Image',
            height: '50px',
            width: '50px',
            url: profileImageUrl,
        },
        {
            type: 'ActionSet',
            actions: [
                {
                    type: 'Action.Submit',
                    title: 'Hide Action card',
                },
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
                    title: 'Click to submit',
                }
            ],
        }
    ],
    type: 'AdaptiveCard',
    version: '1.4'
};

const adaptiveCardTaskModule = {
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: 'Hello: '
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

module.exports = {
    createFetchResponse,
    createSubmitResponse,
    createAuthResponse,
    invokeTaskResponse
};