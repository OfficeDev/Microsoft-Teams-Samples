import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import MeetingTranscriptRecording from "../components/meetingtranscriptrecording";
import AuthStart from "../components/auth-start";
import AuthEnd from "../components/auth-end";
import * as microsoftTeams from "@microsoft/teams-js";

export const AppRoute = () => {

    React.useEffect(() => {
        microsoftTeams.app.initialize();

    }, [])

    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/meetingtranscriptrecording" element={<MeetingTranscriptRecording />} />
                    <Route path="/auth-start" element={<AuthStart />} />
                    <Route path="/auth-end" element={<AuthEnd />} />
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};