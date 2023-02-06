export default function AuthService(teamsClient) {
    return function () {
        return new Promise((resolve, reject) => {
            // teamsClient.GetParticipant();
            teamsClient.app.initialize().then(() => {
                teamsClient.authentication.getAuthToken().then((result) => {
                    resolve(result)
                }).catch((error) => {
                    console.error("Failed to get auth: ", error)
                    reject(error);
                });
            });
        });
    }
}