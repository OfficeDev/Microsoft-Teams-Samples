import IArticle from "../models/article";
import axios from "./axios-decorator";

const baseAxiosUrl = window.location.origin;

/**
* GET all learning content from API
*/
export const getAllLearningContent = async (): Promise<any> => {
    let url = `${baseAxiosUrl}/api/learning/`;
    return await axios.get(url);
}

/**
* GET learning content by id from API
* @param learningId unique id of learning content
*/
export const getLearningContentById = async (learningId: string): Promise<any> => {
    let url = baseAxiosUrl + `/api/learning/${learningId}`;
    return await axios.get(url,undefined, false);
}

/**
* GET learning content by selection type from API
* @param selectiontype type of learning content
*/
export const getLearningContentByType = async (selectiontype: string): Promise<any> => {
    let url = baseAxiosUrl + `/api/learning/selectiontype/${selectiontype}`;
    return await axios.get(url);
}

/**
* POST created new learning content from API
* @param payload data body of the article content
*/
export const createLearningContent = async (payload: IArticle): Promise<any> => {
    let url = baseAxiosUrl + "/api/learning";
    return await axios.post(url, payload);
}

/**
* PATCH update learning content from API
* @param learningId id of learning content
* @param payload data body of the article content
*/
export const updateLearningContent = async (learningId: string, payload: IArticle): Promise<any> => {
    let url = baseAxiosUrl + `/api/learning/${learningId}`;
    return await axios.patch(url, payload);
}

/**
* DELETE learning content by id from API
* @param learningId id of learning content
*/
export const DeleteLearningContentById = async (learningId: string): Promise<any> => {
    let url = baseAxiosUrl + `/api/learning/${learningId}`;
    return await axios.delete(url);
}

/**
* POST article html content from API
* @param query object of html content
*/
export const geArticleHtmlAsync = async (query: object): Promise<any> => {
    let url = baseAxiosUrl + `/api/readarticle`;
    return await axios.post(url, query, undefined, false);
}