import axios, { AxiosResponse } from 'axios';

// API call to get latest value of editor.
export function getLatestEditorValue(questionId: any, meetingId: any): Promise<AxiosResponse<unknown>> {
    return axios.get(`${window.location.origin}/api/editorState?questionId=${questionId}&meetingId=${meetingId}`);
}