import axios, { AxiosResponse } from 'axios';

// API to save meeting context.
export function setMeetingContext(userId, meetingId, tenantId) {
    return axios.get(`${window.location.origin}/Home/IsOrganizer?userId=${userId}&meetingId=${meetingId}&tenantId=${tenantId}`);
}

// API to add meeting agenda.
export function addAgendaTask(taskInfo) {
    return axios.post(`${window.location.origin}/Home/AddNewAgendaPoint`, taskInfo);
}

// API to post agenda in meeting chat.
export function postAgenda() {
    return axios.get(`${window.location.origin}/Home/SendAgenda`);
}