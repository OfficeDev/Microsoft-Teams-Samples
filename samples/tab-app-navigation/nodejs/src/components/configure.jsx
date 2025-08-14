// <copyright file="configure.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";

// Configure page.
const Configure = props => {
    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.notifySuccess();

            microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {
                microsoftTeams.pages.config.setConfig({
                    //entityId : Generating a random id so that each tab instance has a unique ID.
                    entityId: "AppInstance_" + Math.floor(Math.random() * 100 + 1),
                    contentUrl: `${window.location.origin}/default_tab`,
                    suggestedTabName: "Default-Tab",
                    websiteUrl: `${window.location.origin}/default_tab`,
                });

                saveEvent.notifySuccess();
            });

            microsoftTeams.pages.config.setValidityState(true);

        });
    }, []);
    return (
        <div>
            <h2>Navigate within a tab app</h2>
            <h3>This sample shows the tab navigation feature such as navigation between tabs within the app.</h3>
            <p>Please click save button to proceed.</p>
        </div>
    );
};

export default Configure;