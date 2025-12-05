import axios from 'axios';

// API to save meeting context.
export function setMeetingContext(userData) {
  return axios.post(`${window.location.origin}/api/setContext`, userData);
}

// API to update the full agenda list.
export function setAgendaList(agendaList) {
  return axios.post(`${window.location.origin}/api/setAgendaList`, agendaList);
}

// API to post agenda in meeting chat.
export function postAgenda(publishData) {
  return axios.post(`${window.location.origin}/api/sendAgenda`, publishData);
}