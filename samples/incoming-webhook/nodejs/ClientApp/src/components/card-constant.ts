export const DEFAULT_CARD_PAYLOAD = `{
    "@type": "MessageCard",
    "summary": "Task Created",
    "sections": [
        {
            "activityTitle": "Test task",
            "facts": [
                {
                    "name": "Title:",
                    "value": "Test title"
                },
                {
                    "name": "Description:",
                    "value": "Test desc"
                },
                {
                    "name": "Assigned To:",
                    "value": "Test assigned to"
                }
            ]
        }
    ]
}`;