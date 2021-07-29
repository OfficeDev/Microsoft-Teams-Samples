const { StatusCodes } = require('botbuilder');

// Card response for tab fetch request
const showAdaptiveCard = () => {
    console.log("Show Adaptive Card request");

    const res = {
        status: StatusCodes.OK,
        body: {
            "tab": {
                "type": "continue",
                "value": {
                    "cards": [
                        {
                            "card": adaptiveCardWithLink,
                        }
                    ]
                },
            },
        }
    };

    return res;
};

// Adaptive Card to show in task module
const adaptiveCardWithLink = () => ({
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: 'Sample link unfurling'
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
});


module.exports = {
    createFetchResponse,
    createSubmitResponse,
    createAuthResponse,
    invokeTaskResponse,
    taskSubmitResponse
};