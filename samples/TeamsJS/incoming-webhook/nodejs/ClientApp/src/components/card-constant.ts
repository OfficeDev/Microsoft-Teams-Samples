var baseUrl = window.location.origin;
export const DEFAULT_CARD_PAYLOAD = `{
    "@type": "MessageCard",
    "@context": "http://schema.org/extensions",
    "themeColor": "0076D7",
    "summary": "Larry Bryant created a new task",
    "sections": [{
        "activityTitle": "Larry Bryant created a new task",
        "activitySubtitle": "On Project Tango",
        "activityImage": "https://teamsnodesample.azurewebsites.net/static/img/image5.png",
        "facts": [{
            "name": "Assigned to",
            "value": "Megan Bowen"
        },
        {
            "name": "Due date",
            "value": "Wed Jan 05 2022 17:07:18 GMT-0700 (Pacific Daylight Time)"
        },
        {
            "name": "Status",
            "value": "Not started"
        }],
        "markdown": true
    }],
    "potentialAction": [{
        "@type": "ActionCard",
        "name": "Add a comment",
        "inputs": [{
            "@type": "TextInput",
            "id": "comment",
            "isMultiline": false,
            "value": null,
            "title": "Add a comment here for this task"
        }],
        "actions": [{
            "@type": "HttpPOST",
            "name": "Add comment",
            "body": "{\\"comment\\":\\"{{comment.value}}\\"}",
            "bodyContentType": "application/json",
            "target": "${baseUrl}/api/save"
        }]
    }, 
    {
        "@type": "OpenUri",
        "name": "Learn More",
        "targets": [{
            "os": "default",
            "uri": "https://docs.microsoft.com/outlook/actionable-messages"
        }]
    }
]}`;