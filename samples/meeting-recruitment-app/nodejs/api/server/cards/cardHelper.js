// Adaptive Card with assets detail and note.
const getCardForMessage = (message) => ({
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: message
        },
        {
            type: 'ActionSet',
            actions: [
                {
                   type: "Action.OpenUrl",
                   title: "View documents",
                   url: "https://microsoftapc.sharepoint.com/_layouts/15/sharepoint.aspx"
                }
            ]
        }
    ],
    type: 'AdaptiveCard',
    version: '1.4'
});

module.exports = {
    getCardForMessage
};