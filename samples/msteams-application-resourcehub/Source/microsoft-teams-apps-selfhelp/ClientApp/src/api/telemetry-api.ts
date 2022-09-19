import axios from "./axios-decorator";

const baseAxiosUrl = window.location.origin;

/**
 * GET all user telemetry logs from API
 */
export const getAllTelemetry = async (): Promise<any> => {
    let url = baseAxiosUrl + `/api/telemetry`;
    return await axios.get(url);
}