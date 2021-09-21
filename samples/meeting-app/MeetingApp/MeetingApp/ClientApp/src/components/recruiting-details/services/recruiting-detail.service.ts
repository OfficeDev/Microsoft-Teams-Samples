import axios, { AxiosResponse } from 'axios';
import { IQuestionDetails, INoteDetails, IFeedbackDetails } from '../../../types/recruitment.types';
import { IQuestionSet } from '../basic-details/basic-details.types';

// API call to get Candidate details.
export function getCandidateDetails(): Promise<AxiosResponse<unknown>> {
  return axios.get(`${window.location.origin}/api/Candidate`);
}

// API call to save Questions.
export function saveQuestions(questionDetails: IQuestionSet[]): Promise<AxiosResponse<unknown>> {
  return axios.post(`${window.location.origin}/api/Question/insertQuest`, questionDetails);
}

// API call to edit Question.
export function editQuestion(questionDetails: IQuestionSet): Promise<AxiosResponse<unknown>> {
  return axios.post(`${window.location.origin}/api/Question/edit`, questionDetails);
}

// API call to get Questions.
export function getQuestions(meetingId: string): Promise<AxiosResponse<unknown>> {
  return axios.get(`${window.location.origin}/api/Question?meetingId=`+meetingId);
}

// API call to delete Question.
export function deleteQuestion(questionDetails: IQuestionSet): Promise<AxiosResponse<unknown>> {
  return axios.post(`${window.location.origin}/api/Question/delete`, questionDetails);
}

// API call to save feedback.
export function saveFeedback(feedbackDetails: IFeedbackDetails): Promise<AxiosResponse<unknown>> {
  return axios.post(`${window.location.origin}/api/Feedback`, feedbackDetails);
}

// API call to get Questions.
export function getNotes(email: string): Promise<AxiosResponse<unknown>> {
  return axios.get(`${window.location.origin}/api/Notes?email=`+email);
}

// API call to save note.
export function saveNote(noteDetails: INoteDetails): Promise<AxiosResponse<unknown>> {
  return axios.post(`${window.location.origin}/api/Notes`, noteDetails);
}