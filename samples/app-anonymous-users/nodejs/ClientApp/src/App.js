// <copyright file="App.js" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import AppInMeeting from "./components/app-in-meeting";
import Configure from "./components/configure";
import ShareToMeeting from "./components/share-to-meeting";
import * as microsoftTeams from "@microsoft/teams-js";
import ShareView from "./components/shareview";
import FacebookAuthEnd from "./components/facebook-auth-end";
import AuthStart from "./components/auth-start";
import AuthEnd from "./components/auth-end";
export const AppRoute = () => {

    React.useEffect(() => {
        microsoftTeams.app.initialize();

    }, [])

    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<ShareToMeeting />} />
                    <Route path="/appInMeeting" element={<AppInMeeting />} />
                    <Route path="/configure" element={<Configure />} />
                    <Route path="/shareview" element={<ShareView />} />
                    <Route path="/facebook-auth-end" element={<FacebookAuthEnd />} />
                    <Route path="/auth-start" element={<AuthStart />} />
                    <Route path="/auth-end" element={<AuthEnd />} />
              </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};