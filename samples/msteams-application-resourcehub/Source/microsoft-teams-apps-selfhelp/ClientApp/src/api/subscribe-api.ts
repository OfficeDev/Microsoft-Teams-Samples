import ISubscribe from "../models/subscribe";
import axios from "./axios-decorator";

const baseAxiosUrl = window.location.origin;

/**
* POST add or update subscription entity for use from API
* @param payload data body of subscribe user
*/
export const createOrUpdateSubscribeAsync = async (payload: ISubscribe): Promise<any> => {
    let url = baseAxiosUrl + `/api/subscribe/CreateOrUpdateAsync?UserId=${payload.userId}&Status=${payload.status}`;
    return await axios.post(url);
}

/**
* GET subscribe by user id from API 
*/
export const getSubscribeByUserIdAsync = async (): Promise<any> => {
    let url = baseAxiosUrl + "/api/subscribe/GetSubscribeByUserIdAsync";
    return await axios.get(url);
}