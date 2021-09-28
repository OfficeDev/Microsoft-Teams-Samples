// Adaptive Card with assets and note.
const getAdaptiveForMessage = (message) => {
    const adaptiveCard = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        body: [
            {
                type: 'TextBlock',
                text: message,
                weight: 'semilight',
                size: 3
            }
        ],
        actions: [
            {
                type: "openUrl",
                title: "View documents",
                url: "https://microsoftapc.sharepoint.com/_layouts/15/sharepoint.aspx"
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return adaptiveCard;
}

module.exports = {
    getAdaptiveForMessage
};