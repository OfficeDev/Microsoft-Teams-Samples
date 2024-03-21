
import { app, authentication } from "@microsoft/teams-js";

export default function AuthService(teamsClient) {
    return function () {
        return new Promise((resolve, reject) => {
            // teamsClient.GetParticipant();
               app.initialize();
                authentication.getAuthToken().then((result) => {
                    resolve(result)
                }).catch((error) => {
                    console.error("Failed to get auth: ", error)
                    reject(error);
                });
            
        });
    }
}