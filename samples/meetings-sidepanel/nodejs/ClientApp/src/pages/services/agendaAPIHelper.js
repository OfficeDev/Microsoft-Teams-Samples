import {axios} from 'axios';

// API to save meeting context.
export function setMeetingContext(userData) {
  return axios.post(`${window.location.origin}/api/setContext`, userData);
}

// API to add meeting agenda.
export function addAgendaTask(taskInfo) {
  return axios.post(`${window.location.origin}/Home/AddNewAgendaPoint`, taskInfo);
}

// API to post agenda in meeting chat.
export function postAgenda(publishData) {
  return axios.post(`${window.location.origin}/api/sendAgenda`, publishData);
}