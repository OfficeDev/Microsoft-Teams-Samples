// Adaptive Card for meeting start event
const adaptiveCardForMeetingStart = (meetingObject) => ({
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: meetingObject.Title + '- started'
        },
        {
            type: "ColumnSet",
            spacing: "medium",
            columns: [
                {
                    type: "Column",
                    width: 1,
                    items: [
                        {
                            type: 'TextBlock',
                            size: 'Medium',
                            weight: 'Bolder',
                            text: 'Start Time : '
                        }
                    ]
                },
                {
                    "type": "Column",
                    "width": 3,
                    "items": [
                        {
                            type: 'TextBlock',
                            size: 'Medium',
                            text: meetingObject.StartTime
                        }
                    ]
                }
            ]
        },
        {
            type: 'ActionSet',
            actions: [
                {
                    type: "Action.OpenUrl",
                    title: "Join meeting",
                    url: meetingObject.JoinUrl
                }
            ]
        }
    ],
    type: 'AdaptiveCard',
    version: '1.4'
});

// Adaptive Card for meeting end event
const adaptiveCardForMeetingEnd = (meetingObject) => ({
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: meetingObject.Title + '- ended'
        },
        {
            type: "ColumnSet",
            spacing: "medium",
            columns: [
                {
                    type: "Column",
                    width: 1,
                    items: [
                        {
                            type: 'TextBlock',
                            size: 'Medium',
                            weight: 'Bolder',
                            text: 'End Time : '
                        }
                    ]
                },
                {
                    "type": "Column",
                    "width": 3,
                    "items": [
                        {
                            type: 'TextBlock',
                            size: 'Medium',
                            text: meetingObject.EndTime
                        }
                    ]
                }
            ]
        }
    ],
    type: 'AdaptiveCard',
    version: '1.4'
});

module.exports = {
    adaptiveCardForMeetingStart,
    adaptiveCardForMeetingEnd
};