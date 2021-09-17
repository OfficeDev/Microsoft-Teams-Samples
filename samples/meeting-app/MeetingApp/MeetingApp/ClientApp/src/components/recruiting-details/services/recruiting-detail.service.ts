import axios, { AxiosResponse } from 'axios';
import { IQuestionSet } from '../basic-details/basic-details.types';
// Method to get Candidate details.
export function getCandidateDetails(): Promise<AxiosResponse<unknown>> {
  return axios.get(`${window.location.origin}/api/Candidate?email=abc`);
}

// Method to save Questions.
export function saveQuestions(questionDetails: IQuestionSet): Promise<AxiosResponse<unknown>> {
  return axios.post(`${window.location.origin}/api/Question/insertQuest`, questionDetails);
}