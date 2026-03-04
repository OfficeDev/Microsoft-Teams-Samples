import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import Configure from "../components/configure";
import ChannelNotification from "../components/channel-notification";
import TeamNotification from "../components/team-notification";

export const AppRoute = () => {
    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/configure" element={<Configure />}/>
                    <Route path="/channel/:pageId" element={<ChannelNotification />}/>
                    <Route path="/team/:pageId" element={<TeamNotification />}/>
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};