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
                    //entityId : Generating a random id so that each tab instance has a unique ID.
                    entityId: "AppInstance_" + Math.floor(Math.random() * 100 + 1),
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
            <Text size="small" content="This sample app only supports app caching in the side panel, but the feature is available in the following contexts (Chats, Channels, Personal Apps) " weight="semibold" /><br />
            <Text size="small" content="Please click save button to proceed." weight="semibold" />
        </div>
    );
};

export default Configure;