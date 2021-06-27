import axios from "axios";

const clientContext = async(teamsClient, timeout = 10000) => {
    return new Promise((resolve, reject) => {
        let shouldReject = true;
        teamsClient.getContext((teamsContext) => {
            shouldReject = false;
            resolve({ 
                ...teamsContext,
                meetingId: teamsContext.meetingId,
                conversationId: teamsContext.chatId,
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

const getAuthCode = (teamsClient) => {
    return new Promise((resolve, reject) => {
        teamsClient.authentication.getAuthToken({
            successCallback: function (token) {
                resolve(token)
            },
            failureCallback: function (error) {
                console.error("Failed to get auth: ", error)
                reject(error);
            },
        })
    });
}

const getUserProfile = async (authCode, context) => {
    const data = {
        context: context,
    }
    const options = {
        headers: {
            Authorization: `Bearer ${authCode}`
        }
    }
    return axios.post('/api/user/profile', data, options)
}

const getUserPhoto = async (authCode, context) => {
    const data = {
        context: context,
    }
    const options = {
        headers: {
            Authorization: `Bearer ${authCode}`
        }
    }
    return axios.post('/api/user/photo', data, options)
}

export {
    clientContext,
    getAuthCode,
    getUserProfile,
    getUserPhoto
}
