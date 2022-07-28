export default function ContextService(teamsClient, timeout = 10000) {
    return function () {
        return new Promise((resolve, reject) => {
            let shouldReject = true;
            teamsClient.app.initialize().then(() => {
                teamsClient.app.getContext().then((teamsContext) => {
                    shouldReject = false;
                    resolve({
                        ...teamsContext,
                        meetingId: teamsContext.meeting.id,
                        conversationId: teamsContext.chat.id,
                    });
                });
                setTimeout(() => {
                    if (shouldReject) {
                        console.error("Error getting context: Timeout. Make sure you are running the app within teams context and have initialized the sdk");
                        reject("Error getting context: Timeout");
                    }
                }, timeout);
            });
        });
    }
}