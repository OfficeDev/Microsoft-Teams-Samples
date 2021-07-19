const { StatusCodes } = require('botbuilder');
const { GraphClient } = require('../graphClient')

const createAuthResponse = (signInLink) => {
    console.log("Create Auth response")
    const res = {
        status: StatusCodes.OK,
        body: {
            "tab": {
                "type": "auth",
                "suggestedActions":{
                    "actions":[
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

const createFetchResponse = async (tokenResponse, userName) => {
    console.log("Create Invoke response")
    const graphClient = new GraphClient(tokenResponse.token);
    const profile = await graphClient.GetUserProfile();
    console.log(profile);
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
            text: 'Test the tab view with Adaptive card'
        },
        {
            type: 'ActionSet',
            actions: [
                {
                    type: 'Action.Submit',
                    title: 'Hide Action card',
                },
                {
                    type: 'Action.Submit',
                    title: 'Action 2',
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

module.exports = {
    createFetchResponse,
    createSubmitResponse,
    createAuthResponse
};