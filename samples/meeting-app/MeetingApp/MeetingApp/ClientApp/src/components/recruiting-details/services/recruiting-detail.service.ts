import axios, { AxiosResponse } from 'axios';

// Method to get Candidate details.
export function getCandidateDetails(): Promise<AxiosResponse<unknown>> {
  return axios.get('https://9fb2-223-181-133-144.ngrok.io/api/Candidate?email=abc');
}