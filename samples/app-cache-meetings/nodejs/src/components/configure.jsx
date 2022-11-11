// <copyright file="configure.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { useEffect } from "react";
import { Text } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";

// Configure page.
const Configure = props => {
    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.notifySuccess();

            microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {
                microsoftTeams.pages.config.setConfig({
                    entityID: "AppCache",
                    contentUrl: `${window.location.origin}/appCacheTab`,
                    suggestedTabName: "Cache-Tab",
                    websiteUrl: `${window.location.origin}/appCacheTab`,
                });
                saveEvent.notifySuccess();
            });
            microsoftTeams.pages.config.setValidityState(true);
        });
    }, []);
    return (
        <div>
            <Text size="larger" weight="semibold" content="App Caching" /><br />
            <Text size="small" content="This is the test app for app caching. This app only works in the side panel for testing purposes." weight="semibold" /><br />
            <Text size="small" content="Please click save button to proceed." weight="semibold" />
        </div>
    );
};

export default Configure;