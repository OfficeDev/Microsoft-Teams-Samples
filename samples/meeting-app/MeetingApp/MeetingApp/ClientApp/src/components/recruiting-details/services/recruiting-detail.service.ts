import axios, { AxiosResponse } from 'axios';
import { IQuestionDetails } from '../../../types/recruitment.types';
import { IQuestionSet } from '../basic-details/basic-details.types';

// Method to get Candidate details.
export function getCandidateDetails(): Promise<AxiosResponse<unknown>> {
  return axios.get(`${window.location.origin}/api/Candidate?email=abc`);
}

// Method to save Questions.
export function saveQuestions(questionDetails: IQuestionSet): Promise<AxiosResponse<unknown>> {
  return axios.post(`${window.location.origin}/api/Question/insertQuest`, questionDetails);
}

// Method to get Questions.
export function getQuestions(meetingId: string): Promise<AxiosResponse<unknown>> {
  return axios.get(`${window.location.origin}/api/Question?meetingId=`+meetingId);
}

// Method to delete Question.
export function deleteQuestion(questionDetails: IQuestionSet): Promise<AxiosResponse<unknown>> {
  return axios.post(`${window.location.origin}/api/Question/delete`, questionDetails);
}

// Method to save Questions.
export function saveFeedback(questionDetails: IQuestionDetails[]): Promise<AxiosResponse<unknown>> {
  console.log(questionDetails);
  return axios.post(`${window.location.origin}/api/Question/feedback`, questionDetails);
}