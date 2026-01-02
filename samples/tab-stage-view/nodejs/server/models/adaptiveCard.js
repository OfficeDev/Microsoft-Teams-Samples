// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Adaptive Card for tab to show in stage view
const adaptiveCardForTabStageView = (baseUrl) => {
    // Ensure baseUrl doesn't have trailing slash
    const cleanBaseUrl = baseUrl.replace(/\/$/, '');
    // Encode the URL for use in deeplink context parameter
    const contentUrl = encodeURIComponent(cleanBaseUrl + '/content');
    const websiteUrl = encodeURIComponent(cleanBaseUrl + '/content');
    const stageViewContext = encodeURIComponent(JSON.stringify({
        contentUrl: cleanBaseUrl + '/content',
        websiteUrl: cleanBaseUrl + '/content',
        name: 'DemoStageView'
    }));

    return {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        body: [
            {
                type: 'TextBlock',
                size: 'Medium',
                weight: 'Bolder',
                text: 'Click the button to open the url in tab stage view'
            },
            {
                type: 'ActionSet',
                actions: [
                    {
                        type: "Action.Submit",
                        title: "View via card",
                        data: {
                            msteams: {
                                type: "invoke",
                                value: {
                                    type: "tab/tabInfoAction",
                                    tabInfo: {
                                        contentUrl: cleanBaseUrl + "/content",
                                        websiteUrl: cleanBaseUrl + "/content",
                                        name: "Stage view",
                                        entityId: "entityId"
                                    }
                                }
                            }
                        }
                    },
                    {
                        type: "Action.OpenUrl",
                        title: "View via Deep Link",
                        url: `https://teams.microsoft.com/l/stage/${process.env.TeamsAppId}/0?context=${stageViewContext}`
                    }
                ]
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
};

module.exports = {
    adaptiveCardForTabStageView,
};