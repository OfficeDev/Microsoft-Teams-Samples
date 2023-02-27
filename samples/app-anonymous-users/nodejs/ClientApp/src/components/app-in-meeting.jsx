// <copyright file="app-in-meeting.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { Button } from '@fluentui/react-components';

const AppInMeeting = () => {
    
 // Allows us to execute the React code when the component is already placed in the DOM.
    useEffect(() => {
        microsoftTeams.app.initialize();
    }, [])

   // Share the content to meeting stage view.
    const shareSpecificPart = () => {
        var appContentUrl = "";
        microsoftTeams.app.getContext().then((context) => {
            appContentUrl = `${window.location.origin}/shareview?meetingId=${context.meeting.id}`;
            microsoftTeams.meeting.shareAppContentToStage((err, result) => {
                if (result) {
                    // handle success
                    console.log("Shared successfully!");
                }

                if (err) {
                    // handle error
                    alert('Error while sharing stage view' + JSON.stringify(err))
                }
            }, appContentUrl);
        });
    }

    return (
        <div className="share-specific-part-button">
            <div className="tag-container">
                <h3>Share To Stage View</h3>
                <Button appearance="primary" onClick={shareSpecificPart} >Share</Button>
            </div>
        </div>
    );
};

export default AppInMeeting;
