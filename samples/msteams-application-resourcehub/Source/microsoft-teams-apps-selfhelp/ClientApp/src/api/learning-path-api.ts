import ILearningPath from "../models/learning-path";
import axios from "./axios-decorator";

const baseAxiosUrl = window.location.origin;

/**
* GET current user's learning path from API
* @param userid unique id of user
*/
export const getCurrentUserLearningPath = async (userid: string): Promise<any> => {
    let url = baseAxiosUrl + `/api/learningpath/mylearningpath?userid=${userid}`;
    return await axios.get(url);
}

/**
* POST created or update new learning path content from API
* @param payload data body of learning path
*/
export const createOrUpdateLearningPathContent = async (payload: ILearningPath): Promise<any> => {
    let url = baseAxiosUrl + "/api/learningpath/CreateOrUpdateAsync";
    return await axios.post(url, payload);
}