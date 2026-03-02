import axios, { AxiosResponse } from 'axios';

// API call to send card.
export function sendCard(cardDetails: any): Promise<AxiosResponse<unknown>> {
  return axios.post(`${window.location.origin}/api/Send`, cardDetails);
}