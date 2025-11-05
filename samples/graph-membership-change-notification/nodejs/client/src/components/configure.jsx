// <copyright file="configure.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

// import React, { useEffect } from "react";
import { Text, RadioGroup } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";

// Configure page.
const Configure = props => {

    const handleChange = (e, props) => {
        if(props.value==='1'){
            microsoftTeams.app.initialize().then(() => {
                microsoftTeams.app.notifySuccess();
                microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {
                    microsoftTeams.pages.config.setConfig({
                        entityID: "ChannelNotification",
                        contentUrl: `${window.location.origin}/channel/1`,
                        suggestedTabName: "Channel Notification",
                        websiteUrl: `${window.location.origin}/channel/1`,
                    });
                    saveEvent.notifySuccess();
                });
                microsoftTeams.pages.config.setValidityState(true);
            });
        }
    };
    return (
        <div>
            <Text size="larger" weight="semibold" content="Channel Subscription" /><br />
            <Text size="small" content="Please select option for subscription to get notifications" weight="semibold" />
            <RadioGroup
                onCheckedValueChange={handleChange}
                items={[
                    {
                        key: '1',
                        label: 'Channel Subscription',
                        value: '1',
                    }
                ]}
            /> 
            <Text size="small" content="Please click save button to proceed." weight="semibold" />
        </div>
    );
};

export default Configure;