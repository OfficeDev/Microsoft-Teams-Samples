export default function ContextService(teamsClient, timeout = 10000) {
    return function  () {
        return new Promise((resolve, reject) => {
            let shouldReject = true;
            teamsClient.app.initialize().then(() => {
                teamsClient.app.getContext().then((context) => {
                    shouldReject = false;
                    console.log(context);
                    resolve({
                        ...context,
                        meetingId: context.meeting.id,
                        conversationId: context.chat.id,
                        tid: context.user.tenant.id,
                        userObjectId: context.user.id
                    });
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