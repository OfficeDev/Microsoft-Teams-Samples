// <copyright file="configure.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import {  useEffect } from "react";
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
                    contentUrl: `${window.location.origin}/myview`,
                    suggestedTabName: "Teams Tab",
                    websiteUrl: `${window.location.origin}/myview`,
                });
                saveEvent.notifySuccess();
            });
            microsoftTeams.pages.config.setValidityState(true);
        });
    }, []);
    return (
        <Text size="small" content="Please click on save to proceed." weight="semibold" />
    );
};

export default Configure;