import IArticleCheckBox from "../models/articleCheckBox";
import axios from "./axios-decorator";

const baseAxiosUrl = window.location.origin;

/**
* GET all user by id data from API
* @param userId unique id of user
*/
export const getUserByIdDataAsync = async (userId: String|undefined): Promise<any> => {
    let url = baseAxiosUrl + `/api/UserData/users?usersId=${userId}`;
    return await axios.get(url);
}

/**
* GET user role from SG
* @param upn unique identifier
*/
export const getUserRoleAsync = async (upn: string): Promise<any> => {
    let url = baseAxiosUrl + `/api/UserData/userrole?upn=${upn}`;
    return await axios.get(url);
}

/**
* GET all users from API
* @param searchText for learning content search
*/
export const getAllUsersAsync = async (searchText:string): Promise<any> => {
    let url = baseAxiosUrl + `/api/UserData/allusers?query=${searchText}`;
    return await axios.get(url);
}
/**
* GET all teams from API
*/
export const getAllTeamsAsync = async (): Promise<any> => {
    let url = baseAxiosUrl + `/api/UserData/allteams`;
    return await axios.get(url);
}

/**
* POST send notification to user from API
* @param learningEntity selected learning content
* @param titleText of the learning content
*/
export const sendNotificationAsync = async (learningEntity: IArticleCheckBox[],titleText:String): Promise<any> => {
    let url = baseAxiosUrl + `/api/UserData/SendNotification?title=${titleText}`;
    return await axios.post(url,learningEntity);
}