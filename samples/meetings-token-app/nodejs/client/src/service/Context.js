export default function ContextService(teamsClient, timeout = 10000) {
    return function () {
        return new Promise((resolve, reject) => {
            let shouldReject = true;
            teamsClient.getChatMembers(x=> console.log(x));
            teamsClient.getUserJoinedTeams(x=> console.log(x));
            teamsClient.getContext((teamsContext) => {
                shouldReject = false;
                resolve({ 
                    ...teamsContext,
                    meetingId: teamsContext.meetingId,
                    conversationId: teamsContext.chatId,
                    tid: teamsContext.tid,
                    userObjectId: teamsContext.userObjectId
                });
            });
            setTimeout(() => {
                if (shouldReject) {
                    console.error("Error getting context: Timeout. Make sure you are running the app within teams context and have initialized the sdk");
                    reject("Error getting context: Timeout");
                }
            }, timeout);
        });
    }
}