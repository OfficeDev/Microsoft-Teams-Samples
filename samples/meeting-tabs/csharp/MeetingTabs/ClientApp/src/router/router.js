// <copyright file="router.js" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import AppInMeeting from "../components/app-in-meeting";
import Configure from "../components/configure";
import ShareToMeeting from "../components/share-to-meeting";
import * as microsoftTeams from "@microsoft/teams-js";
import ShareView from "../components/shareview";

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
                    <Route path="/" element={<ShareToMeeting />} />
                    <Route path="/appInMeeting" element={<AppInMeeting />} />
                    <Route path="/configure" element={<Configure />} />
                    <Route path="/shareview" element={<ShareView />} />
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};