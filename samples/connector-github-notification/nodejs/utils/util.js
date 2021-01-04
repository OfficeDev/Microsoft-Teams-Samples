// Generates rich connector cards.
module.exports.createNotificationCard = function (payload, serviceUrl) {
    {
        var id = payload.pull_request.id;
        var action = payload.action;
        var title = payload.pull_request.title;
        var user = payload.pull_request.user.login;
        var url = payload.pull_request._links.html.href;
        var repo = payload.pull_request.head.repo.full_name;
        var avatarUrl = payload.sender.avatar_url;
        var ret = {
            "@type": "MessageCard",
            "@context": "http://schema.org/extensions",
            "summary": "Pull request " + id,
            "themeColor": "0078D7",
            "title": "Pull request " + action + " : " + title,
            "sections": [
                {
                    "activityTitle": user,
                    "activitySubtitle": "9/13/2016, 11:46am",
                    "activityImage": avatarUrl,
                    "facts": [
                        {
                            "name": "Pull request #:",
                            "value": id
                        },
                        {
                            "name": "Action:",
                            "value": action
                        },
                        {
                            "name": "Repository:",
                            "value": repo
                        }
                    ],
                    "text": "Description of the pull request"
                }
            ],
            "potentialAction": [
                {
                    "@type": "ActionCard",
                    "name": "Add a comment",
                    "inputs": [
                        {
                            "@type": "TextInput",
                            "id": "comment",
                            "title": "Enter your comment",
                            "isMultiline": true
                        }
                    ],
                    "actions": [
                        {
                            "@type": "HttpPOST",
                            "name": "OK",
                            "target": serviceUrl + "/comment",
                            "body": "{\"comment\":\"{{comment.value}}\"}",
                            "bodyContentType": "application/json",
                        }
                    ]
                },
                {
                    "@type": "HttpPOST",
                    "name": "Merge",
                    "target": serviceUrl + "/mergerequest"
                },
                {
                    "@type": "HttpPOST",
                    "name": "Close",
                    "target": serviceUrl + "/closerequest"
                },
                {
                    "@type": "OpenUri",
                    "name": "View in GitHub",
                    "targets": [
                        { "os": "default", "uri": url }
                    ]
                }
            ]
        }
    }
    return ret;
}

module.exports.createWelcomeMessage = function(repo) {
    {
        var text = "A connector for " + repo + " has been set up";
        var ret = {
            "@type": "MessageCard",
            "summary": "Welcome Message",
            "themeColor": "0078D7",
            "sections": [{ "text": text }]
        };
    }
    return ret;
}