// <copyright file="configure.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect } from "react";
import { Text } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";

// Configure page.
const Configure = props => {
    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.notifySuccess();
            microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {
                microsoftTeams.pages.config.setConfig({
                    entityID: "TeamworkTagsTab",
                    contentUrl: `${window.location.origin}/dashboard`,
                    suggestedTabName: "Manage Team Tag",
                    websiteUrl: `${window.location.origin}/dashboard`,
                });

                saveEvent.notifySuccess();
            });
            microsoftTeams.pages.config.setValidityState(true);
        });
    }, []);

    return (
        <Text size="larger" content="Please click on save to proceed." weight="semibold" />
    );
};

export default Configure;