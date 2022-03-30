import axios, { AxiosResponse } from 'axios';

// API call to send card.
export function getLatestEditorValue(questionId: any, meetingId: any): Promise<AxiosResponse<unknown>> {
    return axios.get(`${window.location.origin}/api/editorState?questionId=${questionId}&meetingId=${meetingId}`);
}