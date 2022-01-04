import axios, { AxiosResponse } from 'axios';

// API call to send card payload.
export function sendCard(cardDetails: any): Promise<AxiosResponse<unknown>> {
    return axios.post(`${window.location.origin}/api/Send`, cardDetails);
}