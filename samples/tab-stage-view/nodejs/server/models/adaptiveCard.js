// Adaptive Card for tab to show in stage view
const adaptiveCardForTabStageView = (baseUrlForOpenUrl) => ({
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
                                    contentUrl:  process.env.BaseUrl + "/content",
                                    websiteUrl: process.env.BaseUrl + "/content",
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
                   url: "https://teams.microsoft.com/l/stage/"+process.env.MicrosoftAppId+"/0?context=%7B%22contentUrl%22%3A%22https%3A%2F%2F"+baseUrlForOpenUrl+"%2Fcontent%22%2C%22websiteUrl%22%3A%22https%3A%2F%2F"+baseUrlForOpenUrl+"%2Fcontent%22%2C%22name%22%3A%22DemoStageView%22%7D"
                }
            ]
        }
    ],
    type: 'AdaptiveCard',
    version: '1.4'
});

module.exports = {
    adaptiveCardForTabStageView,
};