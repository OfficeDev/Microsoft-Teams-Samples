import axios from "axios";

const clientContext = async(teamsClient, timeout = 10000) => {
    return new Promise((resolve, reject) => {
        let shouldReject = true;
        teamsClient.app.getContext().then((teamsContext) => {
            shouldReject = false;
            resolve({ 
                ...teamsContext
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
        teamsClient.authentication.getAuthToken().then((token) => {
            resolve(token)
        })
        .catch((error) => {
            console.error("Failed to get auth: ", error)
            reject(error);
        });
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