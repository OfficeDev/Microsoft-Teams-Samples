import axios from "./axios-decorator";

const baseAxiosUrl = window.location.origin;

/**
 * POST share article from API
 * @param payload object of learning content
 */
export const shareArticleAsync = async (payload: object): Promise<any> => {
    let url = baseAxiosUrl + "/api/sharearticle";
    return await axios.post(url, payload, undefined, false);
}