import axios from "./axios-decorator";

const baseAxiosUrl = window.location.origin;

/**
* POST get bing search results from API
* @param query object of learning content search
*/
export const geBingSearchResultsAsync = async (query: object): Promise<any> => {
    let url = baseAxiosUrl + `/api/bingsearch`;
    return await axios.post(url, query);
}