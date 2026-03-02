// <copyright file="configure.tsx" company="Microsoft Corporation">
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
                    entityID: "App in Meeting Demo",
                    contentUrl: `${window.location.origin}/appInMeeting`,
                    suggestedTabName: "App in meeting",
                    websiteUrl: `${window.location.origin}/appInMeeting`,
                });

                saveEvent.notifySuccess();
            });
        });
    }, []);

    const onClick = () => {
        microsoftTeams.pages.config.setValidityState(true);
    }

    return (
        <header className="header">
            <div className="header-inner-container">
                <div id="divConfig">
                    <br />
                    <input type="radio" name="notificationType" value="Create" onClick={onClick} /> Add App in a meeting
                </div>
            </div>
        </header>
    );
};

export default Configure;