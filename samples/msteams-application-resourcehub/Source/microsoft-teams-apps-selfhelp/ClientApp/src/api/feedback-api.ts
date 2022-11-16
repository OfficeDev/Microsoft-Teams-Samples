import IFeedback from "../models/feedback";
import axios from "./axios-decorator";

const baseAxiosUrl = window.location.origin;

/**
* GET all user feedback from API
*/
export const getAllUserFeedback = async (): Promise<any> => {
    let url = baseAxiosUrl + `/api/feedback`;
    return await axios.get(url);
}

/**
* POST add new user feedback from API
* @param payload data body of feedback
*/
export const addNewFeedback = async (payload: IFeedback): Promise<any> => {
    let url = baseAxiosUrl + "/api/feedback";
    return await axios.post(url, payload, undefined, false);
}