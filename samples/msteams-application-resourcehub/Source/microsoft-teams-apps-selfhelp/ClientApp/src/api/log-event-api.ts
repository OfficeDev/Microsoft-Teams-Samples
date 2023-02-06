import axios from "./axios-decorator";

const baseAxiosUrl = window.location.origin;

/**
* POST create user activity log from API
* @param payload object of log event
*/
export const logCustomEvent = async (payload: object): Promise<any> => {
    let url = baseAxiosUrl + "/api/logevent";
    return await axios.post(url, payload, undefined, false);
}