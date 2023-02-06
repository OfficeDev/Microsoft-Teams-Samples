import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import Configure from "../components/configure";
import ChannelChangeNotification from "../components/channel-notification";
import TeamChangeNotification from "../components/team-notification";

export const AppRoute = () => {
    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/configure" element={<Configure />}/>
                    <Route path="/channel/:pageId" element={<ChannelChangeNotification />}/>
                    <Route path="/team/:pageId" element={<TeamChangeNotification />}/>
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};