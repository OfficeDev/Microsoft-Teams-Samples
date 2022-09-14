import IUserReaction from "../models/user-reaction";
import axios from "./axios-decorator";

const baseAxiosUrl = window.location.origin;

/**
* GET user like/dislike status from API
* @param learningId unique id of learning content
* @param aadId AadId of user
*/
export const getUserReactionByLearningId = async (learningId: string, aadId: string): Promise<any> => {
    let url = baseAxiosUrl + `/api/userreaction/${learningId}?aadid=${aadId}`;
    return await axios.get(url, undefined, false);
}

/**
* POST create or update new learning content reaction from API
* @param payload data body of user reaction
*/
export const createOrUpdateUserReaction = async (payload: IUserReaction): Promise<any> => {
    let url = baseAxiosUrl + "/api/userreaction";
    return await axios.post(url, payload, undefined, false);
}