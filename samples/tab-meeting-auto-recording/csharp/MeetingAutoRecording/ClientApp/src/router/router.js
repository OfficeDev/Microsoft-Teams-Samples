import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import MeetingAutoRecording from "../components/meetingautorecording";
import AuthStart from "../components/auth-start";
import AuthEnd from "../components/auth-end";
import RecordingTranscript from "../components/viewRecordingTranscript ";

export const AppRoute = () => {

    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/meetingautorecording" element={<MeetingAutoRecording />} />
                    <Route path="/auth-start" element={<AuthStart />} />
                    <Route path="/auth-end" element={<AuthEnd />} />
                    <Route path="/RecordingTranscript" element={<RecordingTranscript />} />
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};