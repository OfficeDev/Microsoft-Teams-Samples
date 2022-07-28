export default function AuthService(teamsClient) {
    return function () {
        return new Promise((resolve, reject) => {
            teamsClient.authentication.getAuthToken().then((result) => {
                resolve(result)
            }).catch((error) => {
                console.error("Failed to get auth: ", error)
                reject(error);
            });
        })
    }
}