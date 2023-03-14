// <copyright file="configure.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";

// Handles redirection after successful/failure sign in attempt.
const Configure = props => {

    useEffect(() => {

        microsoftTeams.app.initialize().then(() => {

            microsoftTeams.app.notifySuccess();
            microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {
                microsoftTeams.pages.config.setConfig({
                    entityID: "Anonymous User Support Demo",
                    contentUrl: `${window.location.origin}/appInMeeting`,
                    suggestedTabName: "Anonymous User Support",
                    websiteUrl: `${window.location.origin}/appInMeeting`,
                });

                saveEvent.notifySuccess();
            });
            microsoftTeams.pages.config.setValidityState(true);

        });
    }, []);
    return (
        <header className="header">
            <div className="header-inner-container">
                <div id="divConfig">
                    <h4>Welcome to the app that support anonymous user support in Teams meeting apps.</h4> 
                </div>
            </div>
        </header>
    );
};

export default Configure;