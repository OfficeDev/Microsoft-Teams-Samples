// Adaptive Card with assets detail and note.
const getCardForMessage = (message, actions) => ({
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: message
        }
    ],
    actions: actions,
    type: 'AdaptiveCard',
    version: '1.4'
});

module.exports = {
    getCardForMessage
};