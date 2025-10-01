import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import AppInMeeting from "../components/app-in-meeting";
import Configure from "../components/configure";
import Home from "../components/home";
import ShareToMeeting from "../components/share-to-meeting";
import * as microsoftTeams from "@microsoft/teams-js";
import Done from "../components/done";
import Doing from "../components/doing";
import Todo from "../components/todo";

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
                    <Route path="/" element= { <ShareToMeeting /> }/>
                    <Route path="/appInMeeting" element= { <AppInMeeting /> }/>
                    <Route path="/configure" element= { <Configure /> }/>
                    <Route path="/home" element= { <Home /> }/>
                    <Route path="/doneView" element= { <Done shareSpecificAppContent={(meetingStatus) => {}} /> }/>
                    <Route path="/doingView" element= { <Doing shareSpecificAppContent={(meetingStatus) => {}} /> }/>
                    <Route path="/todoView" element= { <Todo shareSpecificAppContent={(meetingStatus) => {}} />  }/>
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};