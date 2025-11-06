import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import Index from "../components/index";
import AppInMeeting from "../components/app-in-meeting";
import Configure from "../components/configure";
import * as microsoftTeams from "@microsoft/teams-js";


export const AppRoute = () => {
    React.useEffect(() => {
        microsoftTeams.app
            .initialize()
            .then(() => {
            console.log("App.js: initializing client SDK initialized");
            microsoftTeams.app.notifyAppLoaded();
            microsoftTeams.app.notifySuccess();
            })
            .catch((error) => console.error(error));
      }, []);

    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<Index />} />
                    <Route path="/appInMeeting" element= { <AppInMeeting /> }/>
                    <Route path="/configure" element= { <Configure /> }/>
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};